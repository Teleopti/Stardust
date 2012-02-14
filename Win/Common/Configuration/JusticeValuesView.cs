using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Configuration
{
    public class JusticeValuesView
    {
        private readonly IShiftCategory _wrappedCategory;

        public JusticeValuesView(IShiftCategory shiftCategory)
        {
            _wrappedCategory = shiftCategory;
        }
        public string Name
        {
            get { return _wrappedCategory.Description.Name; }
        }

        public double Monday
        {
            get { return _wrappedCategory.DayOfWeekJusticeValues[DayOfWeek.Monday]; }
            set { _wrappedCategory.DayOfWeekJusticeValues[DayOfWeek.Monday] = (int)value; }
        }

        public double Tuesday
        {
            get { return _wrappedCategory.DayOfWeekJusticeValues[DayOfWeek.Tuesday]; }
            set { _wrappedCategory.DayOfWeekJusticeValues[DayOfWeek.Tuesday] = (int)value; }
        }

        public double Wednesday
        {
            get { return _wrappedCategory.DayOfWeekJusticeValues[DayOfWeek.Wednesday]; }
            set { _wrappedCategory.DayOfWeekJusticeValues[DayOfWeek.Wednesday] = (int)value; }
        }

        public double Thursday
        {
            get { return _wrappedCategory.DayOfWeekJusticeValues[DayOfWeek.Thursday]; }
            set { _wrappedCategory.DayOfWeekJusticeValues[DayOfWeek.Thursday] = (int)value; }
        }

        public double Friday
        {
            get { return _wrappedCategory.DayOfWeekJusticeValues[DayOfWeek.Friday]; }
            set { _wrappedCategory.DayOfWeekJusticeValues[DayOfWeek.Friday] = (int)value; }
        }

        public double Saturday
        {
            get { return _wrappedCategory.DayOfWeekJusticeValues[DayOfWeek.Saturday]; }
            set { _wrappedCategory.DayOfWeekJusticeValues[DayOfWeek.Saturday] = (int)value; }
        }

        public double Sunday
        {
            get { return _wrappedCategory.DayOfWeekJusticeValues[DayOfWeek.Sunday]; }
            set { _wrappedCategory.DayOfWeekJusticeValues[DayOfWeek.Sunday] = (int)value; }
        }
    }
}
