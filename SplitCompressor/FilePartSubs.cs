using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace SplitCompressor
{
    public sealed class FilePartSubs
    {
        public static long FilePartCount(string filePart, int partSize)
        {
            using (FileStream stream = new FileStream(filePart, FileMode.Open))
            {
                long fileSize = stream.Length;
                long partCount = fileSize / partSize;
                if (fileSize % partSize > 0)
                {
                    partCount++;
                }
                return partCount;
            }
        }

        public static List<string> GetFilePartPathesSorted(string wholeFilePath, bool includeLastExtension)
        {
            string wholeFileDir = Path.GetDirectoryName(wholeFilePath);
            string wholeFileName = Path.GetFileName(wholeFilePath);
            FilePartPathComparer pathComparer = new FilePartPathComparer(wholeFilePath, includeLastExtension);
            FilePartPathRegex pathRegex = new FilePartPathRegex(wholeFilePath, includeLastExtension);
            string[] allFilePathes = Directory.GetFiles(wholeFileDir);
            List<string> filePathes = new List<string>(allFilePathes.Where(path => pathRegex.IsMatch(path)));
            filePathes.Sort(pathComparer);
            return filePathes;
        }

        public static List<string> GetAllFilePartPathes(string wholeFilePath, bool includeLastExtension)
        {
            List<string> filePartPathes = GetFilePartPathesSorted(wholeFilePath, includeLastExtension);
            if (filePartPathes.Count == 0)
            {
                throw new FileNotFoundException($"File parts not found: {wholeFilePath}");
            }
            string fileExt = "";
            if (includeLastExtension)
            {
                fileExt = Path.GetExtension(filePartPathes[0]);
                foreach (string filePath in filePartPathes)
                {
                    if (!Path.GetExtension(filePath).Equals(fileExt))
                    {
                        throw new FileNotFoundException($"File parts have different extensions: {wholeFilePath}");
                    }
                }
            }
            FilePartPathRegex pathRegex = new FilePartPathRegex(wholeFilePath, includeLastExtension);
            pathRegex.Match(filePartPathes.Last());
            long partCount;
            if (pathRegex.IsLastIndex())
            {
                partCount = pathRegex.GetPartIndex() + 1;
            }
            else
            {
                throw new FileNotFoundException($"File with the last marker in its name not found: {wholeFilePath}");
            }
            List<string> allFilePartPathes = new List<string>();
            for (int partIndex = 0; partIndex < partCount; partIndex++)
            {
                string filePartPath = FilePartPathRegex.FilePartPath(wholeFilePath, partIndex, partCount, fileExt);
                if (!File.Exists(filePartPath))
                {
                    throw new FileNotFoundException($"File not found: {filePartPath}");
                }
                allFilePartPathes.Add(filePartPath);
            }
            return allFilePartPathes;
        }

        public static string FilePathWithoutLastExt(string filePath)
        {
            return $"{Path.GetDirectoryName(filePath)}{Path.DirectorySeparatorChar}" +
                $"{Path.GetFileNameWithoutExtension(filePath)}";
        }
    }
}
