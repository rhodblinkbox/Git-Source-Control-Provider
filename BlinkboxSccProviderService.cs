// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlinkboxSccProviderService.cs" company="blinkbox">
//   BlinkboxSccProviderService
// </copyright>
// <summary>
//   Blinkbox implementation of the <see cref="SccProviderService" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Blinkbox implementation of the <see cref="SccProviderService"/>.
    /// </summary>
    [Guid("C4128D99-1000-41D1-A6C3-704E6C1A3DE2")]
    public class BlinkboxSccProviderService : SccProviderService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlinkboxSccProviderService"/> class.
        /// </summary>
        /// <param name="sccProvider">The SCC provider.</param>
        /// <param name="trackers">The trackers.</param>
        public BlinkboxSccProviderService(BlinkboxSccProvider sccProvider, List<GitFileStatusTracker> trackers) : base(sccProvider, trackers)
        {
        }

        /// <summary>
        /// Checks whether the solution lists projects which are dev deployable.
        /// </summary>
        /// <returns>true if the solution has devDeployable projects.</returns>
        public bool SolutionIsDevDeployable()
        {
            var dte = (EnvDTE.DTE)_sccProvider.GetService(typeof(SDTE));
            return dte.Solution.Globals["DevBuildProjectNames"] != null;
        }
    }
}