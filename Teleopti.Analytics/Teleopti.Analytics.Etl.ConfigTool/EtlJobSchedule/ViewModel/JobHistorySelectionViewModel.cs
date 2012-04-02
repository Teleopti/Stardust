using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.ConfigTool.EtlJobSchedule.ViewModel
{
	public class JobHistorySelectionViewModel : INotifyPropertyChanged
	{
		private readonly CultureInfo _culture;
		private DateTime _startDate;
		private DateTime _endDate;

		public JobHistorySelectionViewModel(IBaseConfiguration baseConfiguration)
		{
			_culture = baseConfiguration.CultureId.HasValue
									? CultureInfo.GetCultureInfo(baseConfiguration.CultureId.Value)
									: CultureInfo.GetCultureInfo("sv-SE");

			SetWeekPeriod();
		}

		private void SetWeekPeriod()
		{
			StartDate = GetFirstDayOfWeek(DateTime.Now.Date, _culture);
			EndDate = StartDate.AddDays(6);

			SetBusinessUnit();
		}

		private void SetBusinessUnit()
		{
			BusinessUnitCollection = new List<BusinessUnitItem>
			                         	{
			                         		new BusinessUnitItem {Id = new Guid("00000000-0000-0000-0000-000000000002"), Name = "<All>"},
			                         		new BusinessUnitItem
			                         			{Id = new Guid("928DD0BC-BF40-412E-B970-9B5E015AADEA"), Name = "TeleoptiCCCDemo"}
			                         	};
			SelectedBusinessUnitId = BusinessUnitCollection[0].Id;
		}

		private static DateTime GetFirstDayOfWeek(DateTime dayInWeek, CultureInfo cultureInfo)
		{
			DayOfWeek firstDay = cultureInfo.DateTimeFormat.FirstDayOfWeek;
			DateTime firstDayInWeek = dayInWeek.Date;
			while (firstDayInWeek.DayOfWeek != firstDay)
				firstDayInWeek = firstDayInWeek.AddDays(-1);

			return firstDayInWeek;
		}

		
		public DateTime StartDate
		{
			get { return _startDate; }
			set
			{
				_startDate = value;
				RaisePropertyChanged("StartDate");
			}
		}

		
		public DateTime EndDate
		{
			get { return _endDate; }
			set
			{
				_endDate = value;
				RaisePropertyChanged("EndDate");
			}
		}

		public IList<BusinessUnitItem> BusinessUnitCollection { get; set; }
		public Guid SelectedBusinessUnitId { get; set; }

		public void PreviousPeriod()
		{
			MovePeriod(-7);
		}

		public void NextPeriod()
		{
			MovePeriod(7);
		}

		private void MovePeriod(int days)
		{
			StartDate = StartDate.AddDays(days);
			EndDate = EndDate.AddDays(days);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void RaisePropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(
					this,
					new PropertyChangedEventArgs(propertyName));

			}
		}
	}
}
