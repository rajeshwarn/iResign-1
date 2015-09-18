using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iResign
{
    public static class ZipArchiveExtensions
    {
        public static string GetFileExtension(this ZipArchiveEntry entry)
        {
            var splits = entry.Name.Split('.');
            return splits.LastOrDefault();
        }
    }
}
