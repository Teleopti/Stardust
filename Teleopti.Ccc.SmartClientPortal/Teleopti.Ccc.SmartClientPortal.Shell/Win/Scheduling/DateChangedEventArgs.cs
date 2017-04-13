using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
    public class DateChangedEventArgs : EventArgs
    {
        public DateOnly NewDate { get; set; }
    }
}
