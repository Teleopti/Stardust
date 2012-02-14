using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.Win.Scheduling
{
    public class DateChangedEventArgs : EventArgs
    {
        public DateTime NewDate { get; set; }
    }
}
