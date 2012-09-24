// -----------------------------------------------------------------------
// <copyright file="PowershellHost.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation.Host;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class PowershellHost : PSHost
    {
        private Guid hostId = Guid.NewGuid();

        public override System.Globalization.CultureInfo CurrentCulture
        {
            get { return System.Threading.Thread.CurrentThread.CurrentCulture; }
        }

        public override System.Globalization.CultureInfo CurrentUICulture
        {
            get { return System.Threading.Thread.CurrentThread.CurrentUICulture; }
        }

        public override void EnterNestedPrompt()
        {
            throw new NotImplementedException();
        }

        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException();
        }

        public override Guid InstanceId
        {
            get
            {
                return hostId;
            }
        }

        public override string Name
        {
            get { return "powershellHost"; }
        }

        public override void NotifyBeginApplication()
        {
            return;
        }

        public override void NotifyEndApplication()
        {
            return;
        }

        public override void SetShouldExit(int exitCode)
        {
            return;
        }

        public override PSHostUserInterface UI
        {
            get
            {
                return null;
            }
        }

        public override Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }
    }
}
