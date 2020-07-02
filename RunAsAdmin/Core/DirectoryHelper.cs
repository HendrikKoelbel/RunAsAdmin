using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunAsAdmin.Core
{
    internal static class DirectoryHelper
    {
        public static bool CheckWriteAccess(string dirPath)
        {
            var testFilePath = Path.Combine(dirPath, $"{Guid.NewGuid()}");

            try
            {
                File.WriteAllText(testFilePath, "");
                File.Delete(testFilePath);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        public static void Reset(string dirPath)
        {
            if (Directory.Exists(dirPath))
                Directory.Delete(dirPath, true);
            Directory.CreateDirectory(dirPath);
        }
    }
}
