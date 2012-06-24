// -----------------------------------------------------------------------
// <copyright file="OnPackageInitialiseArgs.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox.Events
{
    using System;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class OnPackageInitialiseArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the package instance.
        /// </summary>
        /// <value>The package instance.</value>
        public BasicSccProvider PackageInstance { get; set; }

        /// <summary>
        /// Gets or sets the SCC service.
        /// </summary>
        /// <value>The SCC service.</value>
        public SccProviderService SccService { get; set; }
    }
}
