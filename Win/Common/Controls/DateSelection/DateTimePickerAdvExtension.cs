using System.Reflection;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.DateSelection
{
    public static class DateTimePickerAdvExtension
    {
        public static void SetAvailableTimeSpan(this DateTimePickerAdv dateTimePickerAdv, DateOnlyPeriod  dateTimePair)
        {
            //This is done in reflection due to a bug in Synfusion regarding setting Min/Max values with Arabic culture
            dateTimePickerAdv.GetType().GetField("maxValue", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dateTimePickerAdv, dateTimePair.EndDate.Date);
            dateTimePickerAdv.GetType().GetField("minValue", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dateTimePickerAdv, dateTimePair.StartDate.Date);
            YearNumericUpDown yearNumericUpDown = typeof(MonthCalendarAdv).GetField("yearUD", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).GetValue(
                dateTimePickerAdv.Calendar) as YearNumericUpDown;
            if (yearNumericUpDown != null)
            {
                yearNumericUpDown.Minimum = dateTimePickerAdv.CurrentCalendar.GetYear(dateTimePair.StartDate.Date);
                yearNumericUpDown.Maximum = dateTimePickerAdv.CurrentCalendar.GetYear(dateTimePair.EndDate.Date);
            }
        }

        public static void SetAvailableTimeSpan(this MonthCalendarAdv monthCalendarAdv, DateOnlyPeriod dateTimePair)
        {
            //This is done in reflection due to a bug in Synfusion regarding setting Min/Max values with Arabic culture
            monthCalendarAdv.GetType().GetField("maxValue", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(monthCalendarAdv, dateTimePair.EndDate.Date);
            monthCalendarAdv.GetType().GetField("minValue", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(monthCalendarAdv, dateTimePair.StartDate.Date);
            YearNumericUpDown yearNumericUpDown = monthCalendarAdv.GetType().GetField("yearUD", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).GetValue(
                monthCalendarAdv) as YearNumericUpDown;
            if (yearNumericUpDown != null)
            {
                yearNumericUpDown.Minimum = monthCalendarAdv.CurrentCalendar.GetYear(dateTimePair.StartDate.Date);
                yearNumericUpDown.Maximum = monthCalendarAdv.CurrentCalendar.GetYear(dateTimePair.EndDate.Date);
            }
        }
    }
}
