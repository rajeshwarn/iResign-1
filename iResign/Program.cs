using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ClrPlus.Core.Extensions;
using ClrPlus.Crypto;

namespace iResign
{
    class Program
    {
        static readonly string NugetCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NuGet", "Cache");
        static void Main(string[] args)
        {
          
            SignPackagesInPath(NugetCache);

            Console.ReadLine();

        }


        static void SignPackagesInPath(string path)
        {
            var packages = Directory.EnumerateFiles(path, "*.nupkg");

            foreach (var targetPackage in packages)
            {
                string packageName = Path.GetFileName(targetPackage);
                string tempPackagePath = GetAndCreateTemporaryPathForPackage(packageName);
                string signedPackagePath = Path.Combine(GetAndCreateTemporaryPathForPackage("Signed", false), packageName);
                ResignScriptsInPackage(Path.Combine(NugetCache, targetPackage));
                ZipFile.CreateFromDirectory(tempPackagePath, signedPackagePath);
            }

        }
        static string GetAndCreateTemporaryPathForPackage(string packageName, bool deleteExisting = false)
        {
            var path = Path.Combine(Path.GetTempPath(), "iResign", packageName);
            if (Directory.Exists(path) && deleteExisting)
            {
                Directory.Delete(path, true);
            }
            Directory.CreateDirectory(path);
            
            return path;
        }

        static void ResignScriptsInPackage(string packagePath)
        {
            List<string> scriptPaths = new List<string>();
            using (FileStream stream = new FileStream(packagePath, FileMode.Open))
            {
                using (ZipArchive originalArchive = new ZipArchive(stream))
                {
                    Console.WriteLine("{0}:",packagePath);
                    if (!originalArchive.Entries.Any(z => z.GetFileExtension().Contains("ps1")))
                    {
                        Console.WriteLine("No Powershell scripts found in {0}", packagePath);
                        return;
                    }

                    scriptPaths.AddRange(originalArchive.Entries.Where(z => z.GetFileExtension().Contains("ps1")).Select(z=> z.FullName));
                }
            }

            var tempPath = GetAndCreateTemporaryPathForPackage(Path.GetFileName(packagePath),true);
            ZipFile.ExtractToDirectory(packagePath,tempPath);

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
        }
    }
}
