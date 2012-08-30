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
    using System.Runtime.InteropServices;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Blinkbox implementation for the SccProviderService
    /// </summary>
    public partial class SccProviderService : IVsSolutionEvents
    {
        /// <summary>
        /// Caches the solution directory
        /// </summary>
        private string solutionDirectory = null;

        /// <summary>
        /// Occurs when the source control provider is activated or deactivated.
        /// </summary>
        public event EventHandler SourceControlActivatedOrDeactivated;

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
                if (this.solutionDirectory == null)
                {
                    var sol = (IVsSolution)this._sccProvider.GetService(typeof(SVsSolution));
                    string solutionDirectoryPath, solutionFile, solutionUserOptions;

                    if (sol.GetSolutionInfo(out solutionDirectoryPath, out solutionFile, out solutionUserOptions) == VSConstants.S_OK)
                    {
                        this.solutionDirectory = solutionDirectoryPath;
                    }
                }

                return this.solutionDirectory;
            }

            return null;
        }

        /// <summary>
        /// Compares the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="branchName">Name of the branch.</param>
        public void CompareFile(string fileName, string branchName)
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

        #region IVsSolutionEvents interface functions

        /// <summary>
        /// blinkbox implementation of OnAfterOpenSolution
        /// </summary>
        /// <param name="pUnkReserved">The p unk reserved.</param>
        /// <param name="fNewSolution">The f new solution.</param>
        /// <returns>status as an int</returns>
        public int OnAfterOpenSolution([InAttribute] Object pUnkReserved, [InAttribute] int fNewSolution)
        {
            this.SolutionOpen = true;

            // automatic switch the scc provider
            if (!this.Active && !GitSccOptions.Current.DisableAutoLoad)
            {
                this.OpenTracker();
                if (this.trackers.Count > 0)
                {
                    var rscp = (IVsRegisterScciProvider)this._sccProvider.GetService(typeof(IVsRegisterScciProvider));
                    rscp.RegisterSourceControlProvider(GuidList.guidSccProvider);
                }
            }

            this.Refresh();
            return VSConstants.S_OK;
        }

        /// <summary>
        /// blinkbox implementation of OnAfterCloseSolution
        /// </summary>
        /// <param name="pUnkReserved">The p unk reserved.</param>
        /// <returns>status as an int</returns>
        public int OnAfterCloseSolution([In] Object pUnkReserved)
        {
            this.solutionDirectory = null;
            this.SolutionOpen = false;
            this.CloseTracker();
            return VSConstants.S_OK;
        }

        #endregion
    }
}