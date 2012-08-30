// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SccProviderService.bb.cs" company="blinkbox">
//   Blinkbox implementation for the SccProviderService
// </copyright>
// <summary>
//   Blinkbox implementation for the SccProviderService
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc
{
    using System;
    using System.IO;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Blinkbox implementation for the SccProviderService
    /// </summary>
    public partial class SccProviderService
    {
        /// <summary>
        /// Occurs when the source control provider is activated or deactivated.
        /// </summary>
        public event EventHandler SourceControlActivatedOrDeactivated;

        /// <summary>
        /// Occurs when the source control provider is activated or deactivated.
        /// </summary>
        public event EventHandler SolutionOpenedOrClosed; 

        /// <summary>
        /// Gets a value indicating whether a solution is oen].
        /// </summary>
        /// <value><c>true</c> if [solution open]; otherwise, <c>false</c>.</value>
        public bool SolutionOpen { get; private set; }

        /// <summary>
        /// Gets the solution directory.
        /// </summary>
        /// <returns>the path to the solution.</returns>
        public string GetSolutionDirectory()
        {
            if (this.SolutionOpen)
            {
                var sol = (IVsSolution)this._sccProvider.GetService(typeof(SVsSolution));
                string solutionDirectory, solutionFile, solutionUserOptions;

                if (sol.GetSolutionInfo(out solutionDirectory, out solutionFile, out solutionUserOptions) == VSConstants.S_OK)
                {
                    return Path.GetDirectoryName(solutionFile);
                }
            }

            return null;
        }

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