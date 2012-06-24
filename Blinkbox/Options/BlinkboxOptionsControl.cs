// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlinkboxOptionsControl.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// <summary>
//   Summary description for SccProviderOptionsControl.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox.Options
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    /// Generates the UI for the Blinkbox Options Panel.
    /// </summary>
    public class BlinkboxOptionsControl : System.Windows.Forms.UserControl
    {
        /// <summary>
        /// UI components.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// UI Component
        /// </summary>
        private OpenFileDialog openFileDialog;

        /// <summary>
        /// UI Component
        /// </summary>
        private Label gitTfsLabel;

        /// <summary>
        /// UI Component
        /// </summary>
        private TextBox gitTfsTextBox;

        /// <summary>
        /// UI Component
        /// </summary>
        private Button gitTfsBrowseButton;

        /// <summary>
        /// UI Component
        /// </summary>
        private CheckBox launchInVSCheckbox;

        /// <summary>
        /// The parent page
        /// </summary>
        private BlinkboxOptionsPage parentPage;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlinkboxOptionsControl"/> class.
        /// </summary>
        public BlinkboxOptionsControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            this.InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
        }


        #region Component Designer generated code
       
        #endregion

        /// <summary>
        /// Sets OptionsPage.
        /// </summary>
        public BlinkboxOptionsPage OptionsPage
        {
            set
            {
                this.parentPage = value;
            }
        }

        /// <summary>
        /// Saves options.
        /// </summary>
        internal void Save()
        {
            BlinkboxSccOptions.Current.GitTfsPath = gitTfsTextBox.Text;
            BlinkboxSccOptions.Current.LaunchDeployedUrlsInVS = launchInVSCheckbox.Checked;
            BlinkboxSccOptions.Current.SaveConfig();

            var sccProviderService = (SccProviderService)GetService(typeof(SccProviderService));
            if (sccProviderService != null)
            {
                sccProviderService.Refresh();
            }
        }


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.components != null)
                {
                    this.components.Dispose();
                }

                GC.SuppressFinalize(this);
            }

            base.Dispose(disposing);
        }

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.gitTfsLabel = new System.Windows.Forms.Label();
            this.gitTfsTextBox = new System.Windows.Forms.TextBox();
            this.gitTfsBrowseButton = new System.Windows.Forms.Button();
            this.launchInVSCheckbox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            // 
            // label1
            // 
            this.gitTfsLabel.AutoSize = true;
            this.gitTfsLabel.Location = new System.Drawing.Point(3, 10);
            this.gitTfsLabel.Name = "label1";
            this.gitTfsLabel.Size = new System.Drawing.Size(80, 13);
            this.gitTfsLabel.TabIndex = 11;
            this.gitTfsLabel.Text = "Path to Git TFS";
            // 
            // textBox1
            // 
            this.gitTfsTextBox.Location = new System.Drawing.Point(6, 26);
            this.gitTfsTextBox.Name = "textBox1";
            this.gitTfsTextBox.Size = new System.Drawing.Size(283, 20);
            this.gitTfsTextBox.TabIndex = 12;
            // 
            // button1
            // 
            this.gitTfsBrowseButton.Location = new System.Drawing.Point(295, 23);
            this.gitTfsBrowseButton.Name = "button1";
            this.gitTfsBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.gitTfsBrowseButton.TabIndex = 17;
            this.gitTfsBrowseButton.Text = "Browse ...";
            this.gitTfsBrowseButton.UseVisualStyleBackColor = true;
            this.gitTfsBrowseButton.Click += new System.EventHandler(this.GitTfsPathBrowse_Click);
            // 
            // checkBox3
            // 
            this.launchInVSCheckbox.AutoSize = true;
            this.launchInVSCheckbox.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.launchInVSCheckbox.Location = new System.Drawing.Point(6, 68);
            this.launchInVSCheckbox.Name = "checkBox3";
            this.launchInVSCheckbox.Size = new System.Drawing.Size(227, 17);
            this.launchInVSCheckbox.TabIndex = 25;
            this.launchInVSCheckbox.Text = "Launch deployed websites in Visual Studio";
            this.launchInVSCheckbox.UseVisualStyleBackColor = true;
            // 
            // BlinkboxOptionsControl
            // 
            this.AllowDrop = true;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.Controls.Add(this.launchInVSCheckbox);
            this.Controls.Add(this.gitTfsBrowseButton);
            this.Controls.Add(this.gitTfsTextBox);
            this.Controls.Add(this.gitTfsLabel);
            this.Name = "BlinkboxOptionsControl";
            this.Size = new System.Drawing.Size(1559, 604);
            this.Load += new System.EventHandler(this.SccProviderOptionsControl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        /// <summary>
        /// Page Load Event.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void SccProviderOptionsControl_Load(object sender, EventArgs e)
        {
            this.gitTfsTextBox.Text = BlinkboxSccOptions.Current.GitTfsPath;
        }

        /// <summary>
        /// Handles the Click event of the gitTfsPathBrowse control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void GitTfsPathBrowse_Click(object sender, EventArgs e)
        {
            this.OpenFile("git-tfs.exe", this.gitTfsTextBox);
        }

        /// <summary>
        /// Opens the file.
        /// </summary>
        /// <param name="shexe">The shexe.</param>
        /// <param name="textBox">The text box.</param>
        private void OpenFile(string shexe, TextBox textBox)
        {
            this.openFileDialog.FileName = shexe;
            if (this.openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox.Text = this.openFileDialog.FileName;
            }
        }
    }

}
