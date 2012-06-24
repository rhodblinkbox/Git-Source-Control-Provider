// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OnRegisterCommandsArgs.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox.Events
{
    using System;

    /// <summary>
    /// Arguments for the OnRegisterCommands event
    /// </summary>
    public class OnRegisterCommandsArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the VS MenuService.
        /// </summary>
        public Microsoft.VisualStudio.Shell.OleMenuCommandService MenuService { get; set; }
    }
}
