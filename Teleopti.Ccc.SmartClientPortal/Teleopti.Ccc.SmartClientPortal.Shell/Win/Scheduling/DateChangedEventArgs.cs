using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
    public class DateChangedEventArgs : EventArgs
    {
        public DateOnly NewDate { get; set; }
    }
}
