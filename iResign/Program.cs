using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ClrPlus.Core.Extensions;
using ClrPlus.Crypto;

namespace iResign
{
    internal class Program
    {
        private static readonly string NugetCache =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NuGet", "Cache");

        private static void Main(string[] args)
        {
            ResignScriptsInPackages(NugetCache, Path.Combine(Path.GetTempPath(), "iResignedPackages"));
            //todo: make it easier to grab the signed package path
            Console.ReadLine();

        }



        private static void ResignScriptsInPackages(string packagePath, string outputPath)
        {
            var packages = Directory.EnumerateFiles(packagePath, "*.nupkg");


            foreach (var targetPackage in packages)
            {

                List<string> scriptPaths = new List<string>();
                using (FileStream stream = new FileStream(targetPackage, FileMode.Open))
                {
                    using (ZipArchive originalArchive = new ZipArchive(stream))
                    {
                        Console.WriteLine("{0}:", packagePath);
                        if (!originalArchive.Entries.Any(z => z.GetFileExtension().Contains("ps1")))
                        {
                            Console.WriteLine("No Powershell scripts found in {0}", targetPackage);
                            continue;
                        }

                        scriptPaths.AddRange(
                            originalArchive.Entries.Where(z => z.GetFileExtension().Contains("ps1"))
                                .Select(z => z.FullName));


                        var tempPath = Path.GetRandomFileName();
                        originalArchive.ExtractToDirectory(tempPath);

                        foreach (var script in scriptPaths)
                        {
                            var currentFile = Path.Combine(tempPath, script);
                            bool isSigned = Verifier.HasValidSignature(currentFile);
                            Console.WriteLine("{0} is {1}", currentFile, isSigned ? "signed" : "not signed");
                            if (!isSigned)
                            {
                                SignFile.SignFileFromDisk(currentFile);
                            }
                        }

                        var newPackageName = Path.GetFileName(targetPackage);
                        var outputPackagePath = Path.Combine(outputPath, newPackageName);

                       
                        var parent = Path.GetDirectoryName(outputPackagePath);
                        if (!Directory.Exists(parent))
                        {
                            Directory.CreateDirectory(parent);
                        }
                        ZipFile.CreateFromDirectory(tempPath,outputPackagePath);

                    }
                }

            }
        }


    }
}
