// -----------------------------------------------------------------------
// <copyright file="Deploy.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox.Data
{
    using System;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    public class Deployment
    {
        public string BuildLabel { get; set; }

        public string Message { get; set; }

        public string AppUrl { get; set; }

        public string TestRunUrl { get; set; }
    }
}
