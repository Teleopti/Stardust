using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.DateSelection
{
	public partial class DateSelectionCalendar : IDateSelectionControl
	{
		public event EventHandler<DateRangeChangedEventArgs> DateRangeChanged;

		private List<DateTime> _datesVisibleInCalendar = new List<DateTime>();
		private bool _gatherDatesVisibleInCalendar;
		private IList<DateTime> _selectedDates;

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
			if (handler!=null)
			{
				handler.Invoke(this, 
										new DateRangeChangedEventArgs(
											new ReadOnlyCollection<DateOnlyPeriod>(GetSelectedDates())));
			}
		}

		private void RemoveSelectedDatesOutsideCalendarView()
		{
			_gatherDatesVisibleInCalendar = true;
			_datesVisibleInCalendar.Clear();
			monthCalendarAdv1.RefreshCalendar(true);
			_gatherDatesVisibleInCalendar = false;
			var datesToRemove = new List<DateTime>();
			datesToRemove.AddRange(_selectedDates.Where(selectedDate => !_datesVisibleInCalendar.Contains(selectedDate)));

			foreach (DateTime selectedDate in datesToRemove)
			{
				_selectedDates.Remove(selectedDate);
			}
		}

		private void RemoveSelectedDateDuplicates()
		{
			var duplicates = FindDuplicates();
			foreach (IGrouping<DateTime, DateTime> duplicateDate in duplicates)
			{
				_selectedDates.Remove(duplicateDate.Key);
			}
		}

		private IEnumerable<IGrouping<DateTime, DateTime>> FindDuplicates()
		{
			return _selectedDates.GroupBy(g => g)
				.Where(c => c.Count() > 1)
				.ToList();
		}

		private IList<DateTime> SelectedDates
		{
			get
			{
				_selectedDates = new List<DateTime>(monthCalendarAdv1.SelectedDates);
				RemoveSelectedDatesOutsideCalendarView();
				RemoveSelectedDateDuplicates();

				return _selectedDates;
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
			if(SelectedDates.Count > 0)
			{
				var dateTimes = SelectedDates.OrderBy(d => d);
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
		[Browsable(true),DefaultValue(true),Category("Teleopti Behavior")]
		public bool ShowApplyButton
		{
			get { return btnApplyPeriod.Visible; }
			set { btnApplyPeriod.Visible = value;
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
		[Browsable(true),DefaultValue("xxApply"),Localizable(true),Category("Teleopti Texts")]
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

		private void monthCalendarAdv1_DateCellQueryInfo(object sender, Syncfusion.Windows.Forms.Tools.DateCellQueryInfoEventArgs e)
		{
			if (e.DateValue != null && _gatherDatesVisibleInCalendar && !_datesVisibleInCalendar.Contains((DateTime)e.DateValue))
			{
				_datesVisibleInCalendar.Add((DateTime)e.DateValue);
			}
		}
	}
}