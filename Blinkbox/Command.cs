// -----------------------------------------------------------------------
// <copyright file="Command.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
namespace GitScc.Blinkbox
{
    using System.Diagnostics;
    using System.Text;

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
            OutputDataReceived += (sender, args) => this.ReceiveOutput(args);
            ErrorDataReceived += (sender, args) => this.ReceiveOutput(args);
        }

        /// <summary>
        /// Gets OutputText.
        /// </summary>
        public string Output { get; private set; }

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
        public new Command Start()
        {
            NotificationWriter.Write(System.IO.Path.GetFileName(this.StartInfo.FileName + " " + this.StartInfo.Arguments));
            base.Start();

            if (this.WaitUntilFinished)
            {
                WaitForExit();
            }

            this.Output = StandardOutput.ReadToEnd().TrimEnd("\n".ToCharArray());
            this.Error = StandardError.ReadToEnd().TrimEnd("\n".ToCharArray());
            NotificationWriter.Write(this.Output);
            NotificationWriter.Write(this.Error);
            return this;
        }

        /// <summary>
        /// Executes the command and waits for it to return.
        /// </summary>
        /// <returns>
        /// The command.
        /// </returns>
        public Command StartAndWait()
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
            NotificationWriter.Write(args.Data);
        }
    }

    /// <summary>
    /// Executes a command in git.
    /// </summary>
    public class GitCommand : Command
    {
        public GitCommand(string command, string workingDirectory)
            : base(GitBash.GitExePath, command, workingDirectory)
        {
            // Required for various git commands such as diff to prevent a warning.
            StartInfo.EnvironmentVariables.Add("TERM", "msys");
        }

        /// <summary>
        /// Gets Git outputs some information as errors - so combine error and output.
        /// </summary>
        public new string Output
        {
            get
            {
                return base.Output + Error;
            } 
        }
    }
}
