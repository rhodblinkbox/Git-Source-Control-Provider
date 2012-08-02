// -----------------------------------------------------------------------
// <copyright file="Command.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
namespace GitScc.Blinkbox
{
    using System.Diagnostics;

    /// <summary>
    /// Wrapper for a new process.
    /// </summary>
    public class Command : Process
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="workingDirectory">
        /// The working Directory.
        /// </param>
        public Command(string command, string arguments, string workingDirectory)
        {
            StartInfo = new ProcessStartInfo(command, arguments);
            StartInfo.WorkingDirectory = workingDirectory;

            // Hidden process - no window.
            StartInfo.UseShellExecute = false;
            StartInfo.CreateNoWindow = true;
            StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

            // Send output to the pending changes window
            StartInfo.RedirectStandardOutput = true;
            StartInfo.RedirectStandardError = true;
            OutputDataReceived += (sender, args) => NotificationWriter.Write(args.Data);
            ErrorDataReceived += (sender, args) => NotificationWriter.Write(args.Data);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current process should wait for the command to finish.
        /// </summary>
        public bool WaitUntilfinished { get; set; }

        /// <summary>
        /// Executes the command
        /// </summary>
        public new void Start()
        {
            base.Start();
            this.BeginOutputReadLine();
            this.BeginErrorReadLine();

            if (this.WaitUntilfinished)
            {
                WaitForExit();
            }
        }
    }
}
