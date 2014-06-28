using System.Drawing;
using System.Globalization;
using System.Reflection;
using Syncfusion.Drawing;
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

		public static void SetCalendarMetroStyle(this DateTimePickerAdv dateTimePickerAdv)
		{
			dateTimePickerAdv.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			dateTimePickerAdv.Calendar.AllowMultipleSelection = false;
			dateTimePickerAdv.Calendar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			dateTimePickerAdv.Calendar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			dateTimePickerAdv.Calendar.BottomHeight = 25;
			dateTimePickerAdv.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
			dateTimePickerAdv.Calendar.DayNamesColor = System.Drawing.SystemColors.ControlText;
			dateTimePickerAdv.Calendar.DayNamesFont = new System.Drawing.Font("Segoe", 8.25F, System.Drawing.FontStyle.Bold);
			dateTimePickerAdv.Calendar.DaysFont = new System.Drawing.Font("Segoe", 8F);
			dateTimePickerAdv.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
			dateTimePickerAdv.Calendar.Font = new System.Drawing.Font("Segoe", 8F);
			dateTimePickerAdv.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
			dateTimePickerAdv.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			dateTimePickerAdv.Calendar.HeaderEndColor = System.Drawing.Color.White;
			dateTimePickerAdv.Calendar.HeaderHeight = 34;
			dateTimePickerAdv.Calendar.HeaderStartColor = System.Drawing.Color.White;
			dateTimePickerAdv.Calendar.HighlightColor = System.Drawing.Color.White;
			dateTimePickerAdv.Calendar.Iso8601CalenderFormat = false;
			dateTimePickerAdv.Calendar.Location = new System.Drawing.Point(0, 0);
			dateTimePickerAdv.Calendar.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			dateTimePickerAdv.Calendar.Name = "monthCalendar";
			dateTimePickerAdv.Calendar.Office2007Theme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			dateTimePickerAdv.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			dateTimePickerAdv.Calendar.SelectedDates = new System.DateTime[0];
			dateTimePickerAdv.Calendar.ShowWeekNumbers = true;
			dateTimePickerAdv.Calendar.Size = new System.Drawing.Size(209, 174);
			dateTimePickerAdv.Calendar.SizeToFit = true;
			dateTimePickerAdv.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			dateTimePickerAdv.Calendar.TabIndex = 0;
			dateTimePickerAdv.Calendar.ThemedEnabledGrid = true;
			dateTimePickerAdv.Calendar.WeekInterior = new BrushInfo();
			dateTimePickerAdv.Calendar.WeekFont = new System.Drawing.Font("Segoe", 8F, FontStyle.Bold);
			// 
			// 
			// 
			dateTimePickerAdv.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			dateTimePickerAdv.Calendar.TodayButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			dateTimePickerAdv.Calendar.TodayButton.ForeColor = System.Drawing.Color.White;
			dateTimePickerAdv.Calendar.TodayButton.IsBackStageButton = false;
			dateTimePickerAdv.Calendar.TodayButton.Location = new System.Drawing.Point(2, 1);
			dateTimePickerAdv.Calendar.TodayButton.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			dateTimePickerAdv.Calendar.TodayButton.Size = new System.Drawing.Size(134, 21);
			dateTimePickerAdv.Calendar.TodayButton.Text = "Today";
			dateTimePickerAdv.Calendar.TodayButton.UseVisualStyle = true;
		}
	}
}
