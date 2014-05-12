using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Entity;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.ViewModel
{
	public class JobHistorySelectionViewModel : INotifyPropertyChanged
	{
		private readonly CultureInfo _culture;
		private DateTime _startDate;
		private DateTime _endDate;
		private BusinessUnitItem _selectedBusinessUnit;
		private IList<BusinessUnitItem> _businessUnitCollection;
		private bool _showOnlyErrors;

		public JobHistorySelectionViewModel(IBaseConfiguration baseConfiguration)
		{
			_culture = baseConfiguration.CultureId.HasValue
									? CultureInfo.GetCultureInfo(baseConfiguration.CultureId.Value).FixPersianCulture()
									: CultureInfo.GetCultureInfo("sv-SE");

			SetWeekPeriod();

			IsRefreshing = true;
			UpdateBusinessUnitCollection();
			IsRefreshing = false;
			SetFirstBusinessUnitAsSelected();
			ShowOnlyErrors = true;
		}

		public void UpdateBusinessUnitCollection()
		{
			var previousSelectedItem = SelectedBusinessUnit;
			BusinessUnitCollection = BusinessUnitItemMapper.Map();
			SelectedBusinessUnit = BusinessUnitCollection.FirstOrDefault(bu => previousSelectedItem != null && bu.Id == previousSelectedItem.Id);
		}

		private void SetFirstBusinessUnitAsSelected()
		{
			SelectedBusinessUnit = BusinessUnitCollection[0];
		}

		private void SetWeekPeriod()
		{
			StartDate = GetFirstDayOfWeek(DateTime.Now.Date, _culture);
			EndDate = StartDate.AddDays(6);
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

		public IList<BusinessUnitItem> BusinessUnitCollection
		{
			get { return _businessUnitCollection; }
			private set
			{
				_businessUnitCollection = value;
				RaisePropertyChanged("BusinessUnitCollection");
			}
		}

		public BusinessUnitItem SelectedBusinessUnit
		{
			get { return _selectedBusinessUnit; }
			set
			{
				_selectedBusinessUnit = value;
				RaisePropertyChanged("SelectedBusinessUnit");
			}
		}


		public bool ShowOnlyErrors
		{
			get { return _showOnlyErrors; }
			set
			{
				_showOnlyErrors = value;
				RaisePropertyChanged("ShowOnlyErrors");
			}
		}

		public bool IsRefreshing { get; set; }

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
