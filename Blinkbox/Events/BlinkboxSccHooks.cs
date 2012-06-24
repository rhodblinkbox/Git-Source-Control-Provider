// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlinkboxSccHooks.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// <summary>
// Manages event hooks from the package to the blinkbox extensions
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox.Events
{
    using System;

    using Microsoft.VisualStudio.OLE.Interop;

    /// <summary>
    /// Manages event hooks from the package to the blinkbox extensions.
    /// </summary>
    public class BlinkboxSccHooks
    {
        /// <summary>
        /// Hooks into the IOleCommandTarget.QueryStatus call in the package. Can only be set once - it must run syncronously and return a status value. 
        /// </summary>
        private static Func<Guid, OLECMD[], OLECMDF, IntPtr, int> queryCommandStatus = null;

        /// <summary>
        /// Initializes static members of the <see cref="BlinkboxSccHooks"/> class. 
        /// </summary>
        static BlinkboxSccHooks()
        {
            // Initialise event should create a new instance of the BlinkboxScc class
            OnPackageInitialise += (sender, args) => new BlinkboxScc(args.SccService, args.PackageInstance);
        }

        /// <summary>
        /// Event fired when commands are registered during package initialisation.
        /// </summary>
        public static event EventHandler<OnRegisterCommandsArgs> OnRegisterCommands;

        /// <summary>
        /// Event fired when the package initialises.
        /// </summary>
        public static event EventHandler<OnPackageInitialiseArgs> OnPackageInitialise;

        /// <summary>
        /// Event fired after a successful commit.
        /// </summary>
        public static event EventHandler<OnCommitArgs> OnCommit;

        /// <summary>
        /// Gets or sets a single hook into the IOleCommandTarget.QueryStatus call in the package. 
        /// Can only be set once - it must run syncronously and return a status value. 
        /// </summary>
        public static Func<Guid, OLECMD[], OLECMDF, IntPtr, int> QueryCommandStatus
        {
            get
            {
                return queryCommandStatus;
            }

            set
            {
                if (queryCommandStatus != null)
                {
                    throw new Exception("QueryCommandStatus can only be set once");
                }

                queryCommandStatus = value;
            }
        }

        /// <summary>
        /// Trigger for the OnRegisterCommands event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The args.</param>
        public static void TriggerRegisterCommands(object sender, OnRegisterCommandsArgs args)
        {
            if (OnRegisterCommands != null)
            {
                OnRegisterCommands(sender, args);
            }
        }

        /// <summary>
        /// Trigger for the OnRegisterCommands event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The args.</param>
        public static void TriggerOnPackageInitialise(object sender, OnPackageInitialiseArgs args)
        {
            if (OnPackageInitialise != null)
            {
                OnPackageInitialise(sender, args);
            }
        }

        /// <summary>
        /// Triggers the settings CommitSuccessful event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The args.</param>
        internal static void TriggerCommit(object sender, OnCommitArgs args)
        {
            if (OnCommit != null)
            {
                OnCommit(sender, args);
            }
        }
    }
}
