using System;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection
{
    public class DateRangeChangedEventArgs : EventArgs
    {
        private readonly ReadOnlyCollection<DateOnlyPeriod> _selectedDateTimes;

        public DateRangeChangedEventArgs(ReadOnlyCollection<DateOnlyPeriod> dateTimes)
        {
            _selectedDateTimes = dateTimes;
        }

        public ReadOnlyCollection<DateOnlyPeriod> SelectedDates
        {
            get
            {
                return _selectedDateTimes;
            }
        }
    }
}