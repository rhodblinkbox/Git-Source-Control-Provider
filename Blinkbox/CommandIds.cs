// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandIds.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    /// <summary>
    /// Defines the command ids used for blinkbox commands in the Visual Studio environment.
    /// </summary>
    public class CommandIds
    {
        /// <summary>
        /// Identifies the CommitAndTest command.
        /// </summary>
        public const int BlinkboxCommitAndTestId = 0x117;

        /// <summary>
        /// Identifies the GitTfsMenu command.
        /// </summary>
        public const int GitTfsMenu = 0x401;

        /// <summary>
        /// Identifies the GitTfsMenuGroup command.
        /// </summary>
        public const int GitTfsMenuGroup = 0x402;

        /// <summary>
        /// Identifies the GitTfsCheckin command.
        /// </summary>
        public const int GitTfsCheckinButtonId = 0x403;

        /// <summary>
        /// Identifies the GitTfsGetLatest command.
        /// </summary>
        public const int GitTfsGetLatestButtonId = 0x404;
    }
}
