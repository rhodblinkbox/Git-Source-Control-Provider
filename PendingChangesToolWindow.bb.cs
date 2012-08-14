// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PendingChangesToolWindow.bb.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// <summary>
//   Blinkbox implementation inheriting from GitSourceControlProvider.
//   BasicSccProvider has been modified as little as possible, so thats its easier to merge in 3rd party changes.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc
{
    using GitScc.Blinkbox;

    /// <summary>
    /// blinkbox extensions to SccProviderToolWindow.
    /// </summary>
    public partial class PendingChangesToolWindow 
    {
        /// <summary>
        /// Blinkboc commit functionality.
        /// </summary>
        /// <returns>commit data</returns>
        internal CommitData BlinkboxCommit()
        {
            return ((PendingChangesView)control).BlinkboxCommit();
        }
    }
}
