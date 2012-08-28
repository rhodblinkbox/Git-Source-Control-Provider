

namespace GitScc
{
    using System.IO;

    /// <summary>
    /// Blinkbox implementation for the SccProviderService
    /// </summary>
    public partial class SccProviderService
    {
        /// <summary>
        /// Compares the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="branchName">Name of the branch.</param>
        internal void CompareFile(string fileName, string branchName)
        {
            GitFileStatus status = this.GetFileStatus(fileName);
            if (status == GitFileStatus.Modified || status == GitFileStatus.Staged)
            {
                string tempFile = Path.GetFileName(fileName);
                tempFile = Path.Combine(Path.GetTempPath(), tempFile);
                this.CurrentTracker.SaveFileFromRepository(fileName, tempFile, branchName);
                this._sccProvider.RunDiffCommand(tempFile, fileName);
            }
        }
    }
}