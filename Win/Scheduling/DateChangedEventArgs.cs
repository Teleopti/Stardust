using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
    public class DateChangedEventArgs : EventArgs
    {
        public DateOnly NewDate { get; set; }
    }
}
