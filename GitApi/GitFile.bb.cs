using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace GitScc
{
    public partial class GitFile
    {
        public static GitFile FromDiff(string diffLine)
        {
            var parts = diffLine.Split("\t".ToCharArray());
            var filename = parts[1].Split("->".ToCharArray())[0]; // if a rename has occurred the filename is "original -> new"
            var status = parts[0].Substring(0, 1); // If a merge has occurred, the status is "XY", otherwise its just "X"

            return new GitFile { FileName = filename, Status = ParseGitStatus(status) };
        }

        public static GitFileStatus ParseGitStatus(string status)
        {
            switch (status)
            {
                case "M":
                    return GitFileStatus.Modified;

                case "A":
                    return GitFileStatus.Added;

                case "D":
                    return GitFileStatus.Deleted;

                case "R":
                    return GitFileStatus.Renamed;

                case "C":
                    return GitFileStatus.Copied;

                case "U":
                    return GitFileStatus.Modified;

                case "?":
                    return GitFileStatus.NotControlled;

                case "!":
                    return GitFileStatus.Ignored;

                default: 
                    return GitFileStatus.Ignored;
            }
        }
    }
}
