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
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class PowershellHost : PSHost
    {
        private Guid hostId = Guid.NewGuid();
        private RubyPSHostUserInterface _ui = new RubyPSHostUserInterface();

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
                  return _ui; 
            }
        }

        public override Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }
    }

    internal class RubyPSHostUserInterface : PSHostUserInterface
    {
        private StringBuilder _sb;
        private myPSHostRawUI rawui = new myPSHostRawUI();

        public RubyPSHostUserInterface()
        {
            _sb = new StringBuilder();
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            _sb.Append(value);
        }

        public override void Write(string value)
        {
            _sb.Append(value);
        }

        public override void WriteDebugLine(string message)
        {
            _sb.AppendLine("DEBUG: " + message);
        }

        public override void WriteErrorLine(string value)
        {
            _sb.AppendLine("ERROR: " + value);
        }

        public override void WriteLine(string value)
        {
            _sb.AppendLine(value);
        }

        public override void WriteVerboseLine(string message)
        {
            _sb.AppendLine("VERBOSE: " + message);
        }

        public override void WriteWarningLine(string message)
        {
            _sb.AppendLine("WARNING: " + message);
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            return;
        }

        public string Output
        {
            get
            {
                return _sb.ToString();
            }
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message, System.Collections.ObjectModel.Collection<FieldDescription> descriptions)
        {
            throw new NotImplementedException();
        }

        public override int PromptForChoice(string caption, string message, System.Collections.ObjectModel.Collection<ChoiceDescription> choices, int defaultChoice)
        {
            throw new NotImplementedException();
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            throw new NotImplementedException();
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            throw new NotImplementedException();
        }

        public override PSHostRawUserInterface RawUI
        {
            get { return this.rawui; }
        }

        public override string ReadLine()
        {
            throw new NotImplementedException();
        }

        public override System.Security.SecureString ReadLineAsSecureString()
        {
            throw new NotImplementedException();
        }
    }

    public class myPSHostRawUI : PSHostRawUserInterface
    {
       /* public override ConsoleColor ForegroundColor
        public override ConsoleColor BackgroundColor
        public override Size BufferSize
    */
        private StringBuilder sb = new StringBuilder();

        public override Coordinates  CursorPosition
        {
	         get
	         {
	             
		        throw new NotImplementedException(); 
	        }
	          set 
	        { 
		        throw new NotImplementedException(); 
	        }
        }

        public override int  CursorSize
        {
	          get 
	        { 
		        throw new NotImplementedException(); 
	        }
	          set 
	        { 
		        throw new NotImplementedException(); 
	        }
        }

        public override void  FlushInputBuffer()
        {
 	        throw new NotImplementedException();
        }

        public override BufferCell[,]  GetBufferContents(Rectangle rectangle)
        {
 	        throw new NotImplementedException();
        }

        public override bool  KeyAvailable
        {
	        get { throw new NotImplementedException(); }
        }

        public override Size  MaxPhysicalWindowSize
        {
	        get { throw new NotImplementedException(); }
        }

        public override Size  MaxWindowSize
        {
	        get { throw new NotImplementedException(); }
        }

        public override KeyInfo  ReadKey(ReadKeyOptions options)
        {
 	        throw new NotImplementedException();
        }

        public override void  ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
 	        throw new NotImplementedException();
        }

        public override void  SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
 	        throw new NotImplementedException();
        }

        public override void  SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
 	        throw new NotImplementedException();
        }

        public override Coordinates  WindowPosition
        {
	          get 
	        { 
		        throw new NotImplementedException(); 
	        }
	          set 
	        { 
		        throw new NotImplementedException(); 
	        }
        }

        public override Size  WindowSize
        {
	          get 
	        { 
		        throw new NotImplementedException(); 
	        }
	          set 
	        { 
		        throw new NotImplementedException(); 
	        }
        }

        public override string  WindowTitle
        {
	          get 
	        { 
		        throw new NotImplementedException(); 
	        }
	          set 
	        { 
		        throw new NotImplementedException(); 
	        }
        }

        public override ConsoleColor BackgroundColor
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override Size BufferSize
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override ConsoleColor ForegroundColor
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
