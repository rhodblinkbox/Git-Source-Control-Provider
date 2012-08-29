// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PendingChangesToolWindow.bb.cs" company="blinkbox">
//   Blinkbox implementation inheriting from PendingChangesToolWindow.
// </copyright>
// <summary>
//   Blinkbox implementation inheriting from PendingChangesToolWindow.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc
{
    /// <summary>
    /// blinkbox extensions to SccProviderToolWindow.
    /// </summary>
    public partial class PendingChangesToolWindow 
    {
        /// <summary>
        /// Blinkboc commit functionality.
        /// </summary>
        /// <returns>true if the commit was successful</returns>
        internal bool BlinkboxCommit()
        {
            return ((PendingChangesView)this.control).Commit();
        }
    }
}
