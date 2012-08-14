// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommitData.cs" company="blinkbox">
//   OnCommitArgs
// </copyright>
// <summary>
//   Event arguments for the  OnCommit event.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    /// <summary>
    /// Event arguments for the  OnCommit event.
    /// </summary>
    public class CommitData
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
        /// Gets or sets a value indicating whether the commit was succcesful.
        /// </summary>
        public bool Success { get; set; }
    }
}
