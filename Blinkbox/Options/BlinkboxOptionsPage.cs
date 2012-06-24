// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlinkboxOptionsPage.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// <summary>
//   Container window for the blinkbox options page.
//   Called by framework - declared by attribute on BasicSccProvider.
//   The Guid is the window id and must be unique.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox.Options
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    using MsVsShell = Microsoft.VisualStudio.Shell;

    /// <summary>
    /// Container window for the blinkbox options page. 
    /// Called by framework - declared by attribute on BasicSccProvider.
    /// The Guid is the window id and must be unique.
    /// </summary>
    [Guid("717B9FC0-D302-4A72-9C06-B1DC6635CAFE")]
    public class BlinkboxOptionsPage : MsVsShell.DialogPage
    {
        /// <summary>
        /// The options page.
        /// </summary>
        private BlinkboxOptionsControl page;

        /// <summary>
        /// The window.
        /// </summary>
        /// <devdoc>
        ///     The window this dialog page will use for its UI.
        ///     This window handle must be constant, so if you are
        ///     returning a Windows Forms control you must make sure
        ///     it does not recreate its handle.  If the window object
        ///     implements IComponent it will be sited by the 
        ///     dialog page so it can get access to global services.
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected override IWin32Window Window
        {
            get
            {
                this.page = new BlinkboxOptionsControl();
                this.page.Location = new Point(0, 0);
                this.page.OptionsPage = this;
                return this.page;
            }
        }

        /// <summary>
        /// This method is called when VS wants to activate this
        ///     page.  If the Cancel property of the event is set to true, the page is not activated.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnActivate(CancelEventArgs e)
        {
            Trace.WriteLine(string.Format("In OnActivate"));
            base.OnActivate(e);
        }

        /// <summary>
        /// This event is raised when the page is closed. 
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnClosed(EventArgs e)
        {
            Trace.WriteLine(string.Format("In OnClosed"));
            base.OnClosed(e);
        }

        /// <summary>
        ///     This method is called when VS wants to deactivate this
        ///     page.  If true is set for the Cancel property of the event, 
        ///     the page is not deactivated.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnDeactivate(CancelEventArgs e)
        {
            Trace.WriteLine(string.Format("In OnDeactivate"));
            base.OnDeactivate(e);
        }

        /// <summary>
        /// This method is called when VS wants to save the user's 
        /// changes then the dialog is dismissed.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnApply(PageApplyEventArgs e)
        {
            Trace.WriteLine(string.Format("In OnApply"));

            // Preventing the dialog to close if the options were not filled in completely can be done 
            // by setting the ApplyBehavior if needed (e.ApplyBehavior = ApplyKind.Cancel;)
            base.OnApply(e);

            if (this.page != null)
            {
                this.page.Save();
            }
        }
    }
}
