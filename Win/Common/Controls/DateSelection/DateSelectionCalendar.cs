using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.DateSelection
{
	public partial class DateSelectionCalendar : IDateSelectionControl
	{
		public event EventHandler<DateRangeChangedEventArgs> DateRangeChanged;
		private DateTime _previousCalendarValue = DateTime.MinValue;

		public DateSelectionCalendar()
		{
			InitializeComponent();
			monthCalendarAdv1.Culture = CultureInfo.CurrentCulture;
			monthCalendarAdv1.SetAvailableTimeSpan(new DateOnlyPeriod(new DateOnly(DateHelper.MinSmallDateTime),
																	  new DateOnly(DateHelper.MaxSmallDateTime)));
		}

		private void btnApplyPeriod_Click(object sender, EventArgs e)
		{
			var handler = DateRangeChanged;
			if (handler != null)
			{
				handler.Invoke(this,
										new DateRangeChangedEventArgs(
											new ReadOnlyCollection<DateOnlyPeriod>(GetSelectedDates())));
			}
		}

		/// <summary>
		/// Gets the selected dates.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-02
		/// </remarks>
		[Browsable(false)]
		public IList<DateOnlyPeriod> GetSelectedDates()
		{
			IList<DateOnlyPeriod> dateSelectionList = new List<DateOnlyPeriod>();
			if (monthCalendarAdv1.SelectedDates.Length > 0)
			{
				var dateTimes = monthCalendarAdv1.SelectedDates.OfType<DateTime>().OrderBy(d => d);
				TimeSpan difference = TimeSpan.Zero;
				DateTime startDate = dateTimes.ElementAt(0);
				int dateTimesCount = dateTimes.Count();
				for (int i = 1; i < dateTimesCount; i++)
				{
					if (dateTimes.ElementAt(i - 1).Add(difference).AddDays(1) >= dateTimes.ElementAt(i).Add(difference)) continue;
					dateSelectionList.Add(new DateOnlyPeriod(new DateOnly(startDate),
														   new DateOnly(dateTimes.ElementAt(i - 1).Add(difference))));
					startDate = dateTimes.ElementAt(i).Add(difference);
				}
				dateSelectionList.Add(new DateOnlyPeriod(new DateOnly(startDate),
														   new DateOnly(dateTimes.ElementAt(dateTimesCount - 1).Add(difference))));
			}
			return dateSelectionList;
		}

		[Browsable(false)]
		public void SetCurrentDate(DateOnly theDate)
		{
			monthCalendarAdv1.Value = theDate.Date;
		}

		/// <summary>
		/// Gets or sets a value indicating whether [show apply button].
		/// </summary>
		/// <value><c>true</c> if [show apply button]; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-02
		/// </remarks>
		[Browsable(true), DefaultValue(true), Category("Teleopti Behavior")]
		public bool ShowApplyButton
		{
			get { return btnApplyPeriod.Visible; }
			set
			{
				btnApplyPeriod.Visible = value;
				tableLayoutPanel1.RowStyles[1].Height = (value) ? 30F : 0F;
			}
		}

		/// <summary>
		/// Gets or sets the button apply text.
		/// </summary>
		/// <value>The button apply text.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-04-17
		/// </remarks>
		[Browsable(true), DefaultValue("xxApply"), Localizable(true), Category("Teleopti Texts")]
		public string ButtonApplyText
		{
			get { return btnApplyPeriod.Text; }
			set { btnApplyPeriod.Text = value; }
		}

		public override bool HasHelp
		{
			get
			{
				return false;
			}
		}

		private void monthCalendarAdv1_DateChanged(object sender, EventArgs e)
		{
			Debug.WriteLine(string.Format("DateChanged; Value={0}; SeledtedDates={1}", monthCalendarAdv1.Value, GetStringOfSelectedDates()));

			//if (MonthDifference(_previousCalendarValue, monthCalendarAdv1.Value) > 0 && monthCalendarAdv1.SelectedDates.Length > 0)
			//    monthCalendarAdv1.ClearSelection();

			//_previousCalendarValue = monthCalendarAdv1.Value;
		}

		//public static int MonthDifference(DateTime firstDate, DateTime secondDate)
		//{
		//    return Math.Abs((firstDate.Month - secondDate.Month) + 12 * (firstDate.Year - secondDate.Year));
		//}

		private void monthCalendarAdv1_DateSelected(object sender, EventArgs e)
		{
			Debug.WriteLine(string.Format("DateSelected; Value={0}; SeledtedDates={1}", monthCalendarAdv1.Value, GetStringOfSelectedDates()));
		}

		private void monthCalendarAdv1_SelectionChanged(object sender, EventArgs e)
		{
			Debug.WriteLine(string.Format("SelectionChanged; Value={0}; SeledtedDates={1}", monthCalendarAdv1.Value, GetStringOfSelectedDates()));
		}

		private string GetStringOfSelectedDates()
		{
			if (monthCalendarAdv1.SelectedDates == null || monthCalendarAdv1.SelectedDates.Length == 0)
				return "None";

			string dates = string.Empty;
			foreach (var selectedDate in monthCalendarAdv1.SelectedDates)
			{
				dates += selectedDate.ToShortDateString() + " | ";
			}
			return dates;
		}
	}
}