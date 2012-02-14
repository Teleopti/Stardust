using System;

namespace Teleopti.Ccc.DBConverter
{
    public class StatusEventArgs : EventArgs
    {
        private readonly string _statusText;

        public StatusEventArgs(string statusText)
        {
            _statusText = statusText;
        }

        public string StatusText
        {
            get { return _statusText; }
        }
    }
}