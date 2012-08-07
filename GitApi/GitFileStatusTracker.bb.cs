using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GitScc.DataServices;
using NGit;
using NGit.Api;
using NGit.Diff;
using NGit.Dircache;
using NGit.Ignore;
using NGit.Revwalk;
using NGit.Storage.File;
using NGit.Treewalk;
using NGit.Treewalk.Filter;
using System.Diagnostics;

namespace GitScc
{
    public partial class GitFileStatusTracker
    {
        /// <summary>
        /// the name of the head branch.
        /// </summary>
        public const string headBranch = "HEAD";

        /// <summary>
        /// Save the content of a file on the specified branch to the tspecified temp file. 
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="tempFile">
        /// The temp file.
        /// </param>
        /// <param name="branchName">
        /// The branch name.
        /// </param>
        public void SaveFileFromRepository(string fileName, string tempFile, string branchName = headBranch)
        {
            if (!this.HasGitRepository || this.head == null) return;

            if (GitBash.Exists)
            {
                string fileNameRel = GetRelativeFileNameForGit(fileName);
                GitBash.RunCmd(string.Format("show \"{0}:{1}\" > \"{2}\"", branchName, fileNameRel, tempFile), this.GitWorkingDirectory);
            }
            else
            {
                var data = GetFileContent(fileName, branchName);
                using (var binWriter = new BinaryWriter(File.Open(tempFile, System.IO.FileMode.Create)))
                {
                    binWriter.Write(data ?? new byte[] { });
                }
            }
        }

        /// <summary>
        /// Get the content of a file from the specified branch
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="branchName">
        /// The branch name.
        /// </param>
        /// <returns>
        /// </returns>
        public byte[] GetFileContent(string fileName, string branchName = headBranch)
        {
            if (!HasGitRepository || string.IsNullOrEmpty(fileName))
                return null;

            fileName = GetRelativeFileNameForGit(fileName);

            try
            {
                var branch = repository.Resolve(branchName);
                RevTree revTree = branch == null ? null : new RevWalk(repository).ParseTree(branch);
                if (revTree != null)
                {
                    var entry = TreeWalk.ForPath(repository, fileName, revTree);
                    if (entry != null && !entry.IsSubtree)
                    {
                        var blob = repository.Open(entry.GetObjectId(0));
                        if (blob != null) return blob.GetCachedBytes();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine("Get File Content: {0}\r\n{1}", fileName, ex.ToString());
            }

            return null;
        } 
    }
}
