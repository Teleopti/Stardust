using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Schedule;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Meetings.Overview
{
	public partial class CalendarAndTextPanel : UserControl, ILocalized
	{
		public event EventHandler DateChanged;
		public CalendarAndTextPanel()
		{
			InitializeComponent();
			var cultureInfo = CultureInfo.CurrentCulture;
			monthCalendarAdv1.Culture = cultureInfo;

			SetTexts();
			//monthCalendarAdv1.FirstDayOfWeek = (Day)cultureInfo.DateTimeFormat.FirstDayOfWeek;
			monthCalendarAdv1.Iso8601CalenderFormat =
				DateHelper.Iso8601Cultures.Contains(cultureInfo.LCID);
		}

		void monthCalendarAdv1DateSelected(object sender, EventArgs e)
		{
			if (monthCalendarAdv1.SelectedDates.Length > 0)
				monthCalendarAdv1.Value = monthCalendarAdv1.SelectedDates[0];
		}

		void monthCalendarAdv1DateChanged(object sender, EventArgs e)
		{
			OnDateChanged(e);
		}

		public void OnDateChanged(EventArgs e)
		{
			var handler = DateChanged;
			if (handler != null)
				handler(this, e);
		}

		public void SetText(string text)
		{
			textBox1.Text = text;
		}

		public DateTime SelectedDate
		{
			get { return monthCalendarAdv1.Value; }
			set { monthCalendarAdv1.Value = value; }
		}

		public void SelectDateRange(DateSelections dateSelections)
		{
			var dateTimes = new List<DateTime>();
			if (dateSelections != null)
			{
				dateTimes.AddRange(dateSelections.Cast<DateTime>());
			}
			else
			{
				dateTimes.Add(monthCalendarAdv1.Value);
			}

			monthCalendarAdv1.SelectedDates = dateTimes.ToArray();
		}

		public void SetTexts()
		{
			new LanguageResourceHelper().SetTexts(this);
		}

		private void calendarAndTextPanelResize(object sender, EventArgs e)
		{
			Refresh();
			monthCalendarAdv1.Refresh();
		}
	}


}
