using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using Teleopti.Analytics.Etl.Common.Entity;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.ViewModel
{
	public class JobHistorySelectionViewModel : INotifyPropertyChanged
	{
		private readonly CultureInfo _culture;
		private DateTime _startDate;
		private DateTime _endDate;
		private BusinessUnitItem _selectedBusinessUnit;
		private IList<BusinessUnitItem> _businessUnitCollection;
	    private IEtlRunningInformation _runningInformation;
		private bool _showOnlyErrors;
		private readonly IBaseConfiguration _baseConfiguration;
	    private Timer _timer;

		public JobHistorySelectionViewModel(IBaseConfiguration baseConfiguration)
		{
			_baseConfiguration = baseConfiguration;
			_culture = baseConfiguration.CultureId.HasValue
									? CultureInfo.GetCultureInfo(baseConfiguration.CultureId.Value).FixPersianCulture()
									: CultureInfo.GetCultureInfo("sv-SE");

			SetWeekPeriod();

			IsRefreshing = true;
			UpdateBusinessUnitCollection();
			IsRefreshing = false;
			SetFirstBusinessUnitAsSelected();
			ShowOnlyErrors = true;

            _timer = new Timer(tick, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10));
		}

	    private void tick(object o)
	    {
	        UpdateRunningStatus();
	    }

		public void UpdateBusinessUnitCollection()
		{
			//default
			var connectionString = ConfigurationManager.AppSettings["datamartConnectionString"];
			if (_baseConfiguration.JobHelper != null)
				connectionString = _baseConfiguration.JobHelper.SelectedDataSource.Analytics.ConnectionString;
			var previousSelectedItem = SelectedBusinessUnit;
			BusinessUnitCollection = BusinessUnitItemMapper.Map(connectionString);
			SelectedBusinessUnit = BusinessUnitCollection.FirstOrDefault(bu => previousSelectedItem != null && bu.Id == previousSelectedItem.Id);
			if(SelectedBusinessUnit == null)
				SetFirstBusinessUnitAsSelected();
		}
        
	    public void UpdateRunningStatus()
	    {
            var connectionString = ConfigurationManager.AppSettings["datamartConnectionString"];
	        var runningStatus = new RunningStatusRepository(connectionString);
	        RunningStatus = runningStatus.GetRunningJob();
            RaisePropertyChanged(nameof(RunningStatus));
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
				RaisePropertyChanged(nameof(StartDate));
			}
		}


		public DateTime EndDate
		{
			get { return _endDate; }
			set
			{
				_endDate = value;
				RaisePropertyChanged(nameof(EndDate));
			}
		}

	    public IEtlRunningInformation RunningStatus
	    {
            get { return _runningInformation; }
	        private set
	        {
	            _runningInformation = value;
	            RaisePropertyChanged(nameof(RunningStatus));
	        }
	    }

		public IList<BusinessUnitItem> BusinessUnitCollection
		{
			get { return _businessUnitCollection; }
			private set
			{
				_businessUnitCollection = value;
				RaisePropertyChanged(nameof(BusinessUnitCollection));
			}
		}

		public BusinessUnitItem SelectedBusinessUnit
		{
			get { return _selectedBusinessUnit; }
			set
			{
				_selectedBusinessUnit = value;
				RaisePropertyChanged(nameof(SelectedBusinessUnit));
			}
		}


		public bool ShowOnlyErrors
		{
			get { return _showOnlyErrors; }
			set
			{
				_showOnlyErrors = value;
				RaisePropertyChanged(nameof(ShowOnlyErrors));
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
			PropertyChanged?.Invoke(
				this,
				new PropertyChangedEventArgs(propertyName));
		}
	}
}
