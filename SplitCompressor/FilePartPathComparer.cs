using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SplitCompressor
{
    public class FilePartPathComparer : Comparer<string>
    {
        private readonly FilePartPathRegex _regex1;
        private readonly FilePartPathRegex _regex2;

        public FilePartPathComparer(string wholeFilePath, bool includeLastExtension)
        {
            _regex1 = new FilePartPathRegex(wholeFilePath, includeLastExtension);
            _regex2 = new FilePartPathRegex(wholeFilePath, includeLastExtension);
        }

        public override int Compare(string path1, string path2)
        {
            if (_regex1.IsMatch(path1))
            {
                if (_regex2.IsMatch(path2))
                {
                    _regex1.Match(path1);
                    _regex2.Match(path2);
                    long part1Index = _regex1.GetPartIndex();
                    long part2Index = _regex2.GetPartIndex();
                    return part1Index.CompareTo(part2Index);
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (_regex2.IsMatch(path2))
                {
                    return 1;
                }
                else
                {
                    return path1.CompareTo(path2);
                }
            }
        }

    }

}
