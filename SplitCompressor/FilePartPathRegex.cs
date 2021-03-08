using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SplitCompressor
{
    public class FilePartPathRegex
    {
        private const string LAST_MARKER = "_last";
        private readonly Regex _regex;
        private Match _match;
        public Regex Value
        {
            get { return _regex; }
        }

        public FilePartPathRegex(string wholeFilePath, bool includeLastExtension)
        {
            string patternBegin, patternEnd;
            if (wholeFilePath.Contains('\\'))
            {
                patternBegin = wholeFilePath.Replace("\\", "\\\\");
            }
            else
            {
                patternBegin = wholeFilePath;
            }
            if (includeLastExtension)
            {
                patternEnd = "(\\.[A-Za-z]+)";
            }
            else
            {
                patternEnd = "";
            }
            _regex = new Regex($"\\A{patternBegin}\\.([\\d]+({LAST_MARKER})?){patternEnd}\\z", RegexOptions.Compiled);
        }

        public bool IsMatch(string filePath)
        {
            return _regex.IsMatch(filePath);
        }

        public void Match(string filePath)
        {
            _match = _regex.Match(filePath);
        }

        public long GetPartIndex()
        {
            string partIndexStr = _match.Groups[1].Value;
            long partIndex;
            if (!partIndexStr.EndsWith(LAST_MARKER))
            {
                partIndex = long.Parse(partIndexStr);
            }
            else
            {
                partIndex = long.Parse(partIndexStr.Substring(0, partIndexStr.Length - LAST_MARKER.Length));
            }
            return partIndex;
        }

        public bool IsLastIndex()
        {
            string partIndexStr = _match.Groups[1].Value;
            return partIndexStr.EndsWith(LAST_MARKER);
        }

        public static string ArchivePartPath(string archivePath, long partIndex, long partCount)
        {
            string dstFileExt = Path.GetExtension(archivePath);
            string dstFileWithoutExt = $"{Path.GetDirectoryName(archivePath)}" +
                $"{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(archivePath)}";
            string lastMarker = partIndex < partCount - 1 ? "" : LAST_MARKER;
            string filePartPath = $"{dstFileWithoutExt}.{partIndex}{lastMarker}{dstFileExt}";
            return filePartPath;
        }

        public static string FilePartPath(string wholeFilePath, long partIndex, long partCount, string lastExt)
        {
            string lastMarker = partIndex < partCount - 1 ? "" : LAST_MARKER;
            return $"{wholeFilePath}.{partIndex}{lastMarker}{lastExt}";
        }
    }
}
