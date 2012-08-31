// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommitData.cs" company="blinkbox">
//   OnCommitArgs
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox.Data
{
    /// <summary>
    /// Contains properties of a commit.
    /// </summary>
    public struct CommitData
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
        /// Gets or sets a value indicating whether the commit was successful.
        /// </summary>
        public bool Success { get; set; }
    }
}
