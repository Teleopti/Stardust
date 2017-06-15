using System;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Event args to use for status updates (for example when loading or transforming information)
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-05-13
    /// </remarks>
    public class StatusChangedEventArgs : EventArgs
    {
        private string _statusText;

        /// <summary>
        /// Gets or sets the status text.
        /// </summary>
        /// <value>The status text.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-13
        /// </remarks>
        public string StatusText
        {
            get { return _statusText; }
            set { _statusText = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusChangedEventArgs"/> class.
        /// </summary>
        /// <param name="statusText">The status text.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-28
        /// </remarks>
        public StatusChangedEventArgs(string statusText)
        {
            _statusText = statusText;
        }
    }
}
