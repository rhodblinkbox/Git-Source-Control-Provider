// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SccCommand.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    using System.Diagnostics;

    /// <summary>
    /// Runs a command in a new process.
    /// </summary>
    public class SccCommand : Process
    {
        /// <summary>
        /// Instance of the <see cref="NotificationService"/>
        /// </summary>
        private readonly NotificationService notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SccCommand"/> class.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="arguments">The arguments.</param>
        public SccCommand(string command, string arguments)
        {
            var sccHelper = BasicSccProvider.GetServiceEx<SccHelperService>();
            this.notificationService = BasicSccProvider.GetServiceEx<NotificationService>();

            this.StartInfo = new ProcessStartInfo(command, arguments);
            StartInfo.WorkingDirectory = sccHelper.GetWorkingDirectory();

            // Hidden process - no window.
            StartInfo.UseShellExecute = false;
            StartInfo.CreateNoWindow = true;
            StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            StartInfo.LoadUserProfile = true;

            // Send output to the pending changes window
            StartInfo.RedirectStandardOutput = true;
            StartInfo.RedirectStandardError = true;
            this.OutputDataReceived += (sender, args) => this.ReceiveOutput(args);
            this.ErrorDataReceived += (sender, args) => this.ReceiveOutput(args);
        }

        /// <summary>
        /// Gets OutputText.
        /// </summary>
        public string Output { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether output is sent to the notification window.
        /// </summary>
        public bool Silent { get; set; }

        /// <summary>
        /// Gets ErrorText.
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current process should wait for the command to finish.
        /// </summary>
        public bool WaitUntilFinished { get; set; }

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <returns>
        /// The command.
        /// </returns>
        public new SccCommand Start()
        {
            if (!this.Silent)
            {
                this.notificationService.AddMessage(System.IO.Path.GetFileName(this.StartInfo.FileName) + " " + this.StartInfo.Arguments);
            }

            base.Start();

            if (this.WaitUntilFinished)
            {
                this.WaitForExit();
            }

            this.Output = StandardOutput.ReadToEnd().TrimEnd("\n".ToCharArray());
            this.Error = StandardError.ReadToEnd().TrimEnd("\n".ToCharArray());

            if (!this.Silent)
            {
                this.notificationService.AddMessage(this.Output);
                this.notificationService.AddMessage(this.Error);
            }

            return this;
        }

        /// <summary>
        /// Executes the command and waits for it to return.
        /// </summary>
        /// <returns>
        /// The command.
        /// </returns>
        public SccCommand StartAndWait()
        {
            this.WaitUntilFinished = true;
            return this.Start();
        }

        /// <summary>
        /// Receives output from the process
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private void ReceiveOutput(DataReceivedEventArgs args)
        {
            this.notificationService.AddMessage(args.Data);
        }
    }
}
