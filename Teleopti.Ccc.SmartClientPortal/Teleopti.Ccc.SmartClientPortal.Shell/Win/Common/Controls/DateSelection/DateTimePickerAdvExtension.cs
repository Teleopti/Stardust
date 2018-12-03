using System.Drawing;
using System.Globalization;
using System.Reflection;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection
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
		/// Ola 2014-09-01 it is actually all cultures with shortestdaynames that is just one letter that has this problem
		/// just let us use hardcoded with everywhere
		/// </remarks>
		public static void SetCultureInfoSafe(this DateTimePickerAdv dateTimePickerAdv, CultureInfo cultureInfo)
		{

			dateTimePickerAdv.Culture = cultureInfo;
			dateTimePickerAdv.Calendar.Culture = CultureInfo.CurrentCulture;

			dateTimePickerAdv.Calendar.Iso8601CalenderFormat =
				DateHelper.Iso8601Cultures.Contains(cultureInfo.LCID);

			
			dateTimePickerAdv.CalendarSizeToFit = false;
			dateTimePickerAdv.CalendarSize = new Size(240, 175);
			dateTimePickerAdv.Calendar.TodayButton.Text = Resources.Today;
			dateTimePickerAdv.Calendar.NoneButton.Text = Resources.None;
			dateTimePickerAdv.Calendar.WeekFont = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
			dateTimePickerAdv.Calendar.WeekInterior = new BrushInfo();
			dateTimePickerAdv.Calendar.WeekTextColor = Color.FromArgb(255, 153, 51);
		}

		public static void SetCalendarMetroStyle(this DateTimePickerAdv dateTimePickerAdv)
		{
			dateTimePickerAdv.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			dateTimePickerAdv.Calendar.BorderColor = Color.FromArgb(209, 211, 212);
			dateTimePickerAdv.Calendar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			dateTimePickerAdv.Calendar.DayNamesColor = Color.FromArgb(0, 153, 255);
			dateTimePickerAdv.Calendar.DayNamesFont = new Font("Segoe", 9F);
			dateTimePickerAdv.Calendar.DaysFont = new Font("Segoe", 9F);
			dateTimePickerAdv.Calendar.Font = new Font("Segoe", 8F);
			dateTimePickerAdv.Calendar.ForeColor = SystemColors.ControlText;
			dateTimePickerAdv.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			dateTimePickerAdv.Calendar.HeaderEndColor = Color.White;
			dateTimePickerAdv.Calendar.HeaderStartColor = Color.White;
			dateTimePickerAdv.Calendar.HighlightColor = Color.White;
			dateTimePickerAdv.Calendar.Location = new Point(0, 0);
			dateTimePickerAdv.Calendar.MetroColor = Color.FromArgb(0, 153, 255);
			dateTimePickerAdv.Calendar.ShowWeekNumbers = true;
			dateTimePickerAdv.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			dateTimePickerAdv.Calendar.TabIndex = 0;
			dateTimePickerAdv.Calendar.ThemedEnabledGrid = true;
			dateTimePickerAdv.Calendar.WeekFont = new Font("Segoe", 9F);
			dateTimePickerAdv.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			dateTimePickerAdv.Calendar.TodayButton.ForeColor = Color.White;
			dateTimePickerAdv.Calendar.TodayButton.IsBackStageButton = false;
			dateTimePickerAdv.Calendar.TodayButton.UseVisualStyle = true;
		}
	}
}
