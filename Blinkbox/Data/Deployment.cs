// -----------------------------------------------------------------------
// <copyright file="Deploy.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    public class Deployment
    {
        public string Version { get; set; }

        public string Message { get; set; }

        public string AppUrl { get; set; }

        public string TestRunUrl { get; set; }
    }
}
