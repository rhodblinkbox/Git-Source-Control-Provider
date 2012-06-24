// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OnCommitArgs.cs" company="blinkbox">
//   OnCommitArgs
// </copyright>
// <summary>
//   Event arguments for the  OnCommit event.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox.Events
{
    using System;

    /// <summary>
    /// Event arguments for the  OnCommit event.
    /// </summary>
    public class OnCommitArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets Message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets CommitHash.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Gets or sets CommitId.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the commit was succcesful.
        /// </summary>
        public bool Success { get; set; }
    }
}
