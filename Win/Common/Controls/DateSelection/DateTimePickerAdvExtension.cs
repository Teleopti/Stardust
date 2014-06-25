using System.Drawing;
using System.Globalization;
using System.Reflection;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Security.Principal;
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
					yearNumericUpDown.Minimum = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetYear(dateTimePair.StartDate.Date);
					yearNumericUpDown.Maximum = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetYear(dateTimePair.EndDate.Date);
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
					yearNumericUpDown.Minimum = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetYear(dateTimePair.StartDate.Date);
					yearNumericUpDown.Maximum = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetYear(dateTimePair.EndDate.Date);
			}
		}

		/// <summary>
		/// Sets the culture on safe mode by taking some known bugs into consideration.
		/// </summary>
		/// <remarks>
		/// Fixed issues: 
		/// - bugfix 22929: Bulgarian date format not displayed well in most calendar controls:
		/// seems that CalendarSizeToFit property does not work with the BG culture
		/// - taking care of ISO8601 weeknumbers
		/// </remarks>
		public static void SetCultureInfoSafe(this DateTimePickerAdv dateTimePickerAdv, CultureInfo cultureInfo)
		{

			dateTimePickerAdv.Culture = cultureInfo;
			dateTimePickerAdv.Calendar.Culture = CultureInfo.CurrentCulture;

			dateTimePickerAdv.Calendar.Iso8601CalenderFormat =
				DateHelper.Iso8601Cultures.Contains(cultureInfo.LCID);

			if (cultureInfo.LCID == 1026
				&& dateTimePickerAdv.CalendarSizeToFit )
			{
				dateTimePickerAdv.CalendarSizeToFit = false;
				dateTimePickerAdv.CalendarSize = new Size(205, 175);
			}
			//when right to left the calendar get to small so the numbers are not displayed correctly
			if (((IUnsafePerson)TeleoptiPrincipal.Current).Person.PermissionInformation.RightToLeftDisplay)
			{
				dateTimePickerAdv.CalendarSizeToFit = false;
				dateTimePickerAdv.CalendarSize = new Size(205, 175);
			}
		}
	}
}
