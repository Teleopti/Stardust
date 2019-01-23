using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    public class ForecastWorkflowModel
    {
        private IWorkload _workload;
        private readonly IForecastWorkflowDataService _dataService;
        private IScenario _scenario;
        private DateOnlyPeriod _workPeriod;
        private IList<ISkillDay> _skillDays;
        private IList<IValidatedVolumeDay> _validatedVolumeDays;
        private IList<ITaskOwner> _workloadDays;
        private IList<ITaskOwner> _workloadDaysWithStatistics;
        private IList<ITaskOwner> _workloadDaysWithoutOutliers;
        private TaskOwnerPeriod _taskOwnerPeriod;
        private DateOnly _latestValidateDay;
        private DateOnlyPeriod _compareHistoricPeriod;
        private DayOfWeeks _dayOfWeeks;
        private IList<DateOnlyPeriod> _selectedHistoricTemplatePeriod;
        private IList<IOutlier> _outliers;
        private IDictionary<DateOnly, IOutlier> _outliersByWorkDate;
        private readonly IFilteredData _filteredDates = new FilteredData();

        public ForecastWorkflowModel(IWorkload workload, IForecastWorkflowDataService dataService)
        {
            _workload = workload;
            _dataService = dataService;
        }

        public IList<IOutlier> Outliers
        {
            get { return _outliers; }
        }

        public DateOnlyPeriod CompareHistoricPeriod
        {
            get { return _compareHistoricPeriod; }
        }

        public TaskOwnerPeriod TaskOwnerPeriod
        {
            get { return _taskOwnerPeriod; }
        }

        public IList<ITaskOwner> WorkloadDaysWithoutOutliers
        {
            get { return _workloadDaysWithoutOutliers; }
        }

        public IList<ITaskOwner> WorkloadDaysWithStatistics
        {
            get { return _workloadDaysWithStatistics; }
        }

        public IList<ITaskOwner> WorkloadDays
        {
            get { return _workloadDays; }
        }

        public IWorkload Workload
        {
            get { return _workload; }
        }

        public DateOnlyPeriod WorkPeriod
        {
            get { return _workPeriod; }
        }

        public IList<ISkillDay> SkillDays
        {
            get { return _skillDays; }
        }

        public IScenario Scenario
        {
            get { return _scenario; }
        }

        public IList<IValidatedVolumeDay> ValidatedVolumeDays
        {
            get { return _validatedVolumeDays; }
        }

        public DayOfWeeks DayOfWeeks
        {
            get { return _dayOfWeeks; }
        }

        public IDictionary<DateOnly, IOutlier> OutliersByWorkDate
        {
            get { return _outliersByWorkDate; }
        }

        public void InitializeOutliers()
        {
            _outliers = _dataService.InitializeOutliers(_workload);
        }

        public void InitializeDefaultScenario()
        {
            _scenario = _dataService.InitializeDefaultScenario();
        }

        public void InitializeWorkPeriod(IScenario scenario)
        {
            _workPeriod = _dataService.InitializeWorkPeriod(scenario, _workload);
        }

        public void InitializeWorkloadDays(IScenario scenario)
        {
            _workloadDays = _dataService.GetWorkloadDays(scenario, _workload, _workPeriod);
        }

        public void InitializeWorkloadDaysWhenLocked()
        {
            var workloadDayHelper = new WorkloadDayHelper();
            _workloadDays = workloadDayHelper
                .GetWorkloadDaysFromSkillDays(_skillDays, _workload)
                .OfType<ITaskOwner>()
                .ToList();
        }

        public void ReloadFilteredWorkloadDayTemplates(IFilteredData filteredDates, int templateIndex)
        {
            _filteredDates.Merge(filteredDates);
			_dataService.ReloadFilteredWorkloadTemplates(_selectedHistoricTemplatePeriod, _filteredDates.FilteredDateList(), _workload, templateIndex);
        }

        public void InitializeWorkloadDaysWithStatistics(IList<DateOnlyPeriod> periodList)
        {
			var workloadDaysWithStatistics = new List<ITaskOwner>();
			foreach (var period in periodList.OrderBy(d => d.StartDate))
			{
				workloadDaysWithStatistics.AddRange(_dataService.GetWorkloadDaysWithStatistics(period, _workload, _scenario, _validatedVolumeDays));
			}
        	_workloadDaysWithStatistics = workloadDaysWithStatistics;
        }

        public void InitializeWorkloadDaysWithoutOutliers()
        {
            if (_workloadDaysWithStatistics == null)
            {
                _workloadDaysWithStatistics = new List<ITaskOwner>(); //todo, dont know just moving code now,,,
                _workloadDaysWithoutOutliers = new List<ITaskOwner>();
            }
            else
            {
                var period = new DateOnlyPeriod(_workloadDaysWithStatistics.Min(t => t.CurrentDate), _workloadDaysWithStatistics.Max(t => t.CurrentDate));
                _workloadDaysWithoutOutliers = StatisticHelper.ExcludeOutliersFromStatistics(period, _outliers, _workloadDaysWithStatistics);
            }
        }

        public void InitializeTaskOwnerPeriod(DateOnly dateTime, TaskOwnerPeriodType taskOwnerPeriodType)
        {
            InitializeWorkloadDaysWithoutOutliers();
            _taskOwnerPeriod = new TaskOwnerPeriod(dateTime, _workloadDaysWithoutOutliers, taskOwnerPeriodType);
        }

        public bool HasOnlyOutliersInStatisticsSelection()
        {
            return _workloadDaysWithStatistics.Count > 0 && _workloadDaysWithoutOutliers.Count == 0;
        }

        public void InitializeLatestValidateDay(bool locked)
        {
            if (locked)
            {
                _latestValidateDay = _workPeriod.StartDate;
            }
            else
            {
                _latestValidateDay = _dataService.FindLatestValidateDay(_workload);

                var dateToday = DateOnly.Today;
                var period = _latestValidateDay.AddDays(1) > dateToday
                            ? new DateOnlyPeriod(_latestValidateDay, _latestValidateDay.AddDays(1))
                            : new DateOnlyPeriod(_latestValidateDay.AddDays(1), dateToday);

                _workPeriod = period;
            }

            _compareHistoricPeriod = new DateOnlyPeriod(new DateOnly(CultureInfo.CurrentCulture.Calendar.AddMonths(_latestValidateDay.Date, -2)), _latestValidateDay);
        }

        public void InitializeValidatedVolumeDays()
        {
            var tempDays = _dataService.FindRange(_workPeriod, _workload, GetWorkloadDaysWithStatistics(_workPeriod));

            if (tempDays != null)
            {
                _validatedVolumeDays = tempDays;
            }
        }

        public void InitializeDayOfWeeks()
        {
            var taskOwnerDays = GetWorkloadDaysWithStatistics(_compareHistoricPeriod);
            var taskOwnerPeriod = new TaskOwnerPeriod(DateOnly.Today, taskOwnerDays, TaskOwnerPeriodType.Other);
            _dayOfWeeks = new DayOfWeeks(taskOwnerPeriod, new DaysOfWeekCreator());
        }

        private IList<IWorkloadDayBase> GetWorkloadDaysWithStatistics(DateOnlyPeriod period)
        {
            return _dataService.LoadStatisticData(period, _workload);
        }

        public IList<TaskOwnerPeriod> CreateYearTaskOwnerPeriods()
        {
            InitializeWorkloadDaysWithoutOutliers();
            var taskOwnerHelper = new TaskOwnerHelper(_workloadDaysWithoutOutliers);
            return taskOwnerHelper.CreateYearTaskOwnerPeriods(CultureInfo.CurrentCulture.Calendar);
        }

        public void LoadWorkloadDayTemplates(IList<DateOnlyPeriod> dates)
        {
            _dataService.LoadWorkloadTemplates(dates, _workload);
        }

        public void SaveWorkflow()
        {
            _dataService.SaveWorkflow(_workload, _workloadDays, _validatedVolumeDays);
        }

        public void InitializeCompareHistoricPeriod(DateOnlyPeriod period)
        {
            _compareHistoricPeriod = period;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IList<IWorkloadDayBase> GetWorkloadDaysForTemplatesWithStatistics()
        {
            var workloadDays = new List<IWorkloadDayBase>();
            foreach (var period in _selectedHistoricTemplatePeriod)
            {
                var statisticData = _dataService.LoadStatisticData(period, _workload);
                workloadDays.AddRange(statisticData);
            }
            return workloadDays;
        }

        public void SetSelectedHistoricTemplatePeriod(IList<DateOnlyPeriod> selectedDates)
        {
            _selectedHistoricTemplatePeriod = selectedDates;
        }

        public void SetWorkPeriod(DateOnlyPeriod selectedDate)
        {
            _workPeriod = selectedDate;
        }

        public void Initialize(IScenario scenario, DateOnlyPeriod period, IList<ISkillDay> skillDays)
        {
            _workPeriod = period;
            _skillDays = skillDays;
            _scenario = scenario;
        }

        public void InitializeOutliersByWorkDate()
        {
            if (_outliers == null)
                InitializeOutliers();

            _outliersByWorkDate = Outlier.GetOutliersByDates(_workPeriod, _outliers);
        }

        public void AddOutlier(IOutlier outlier)
        {
            _dataService.AddOutlier(outlier);
            InitializeOutliers();
        }

        public void RemoveOutlier(IOutlier outlier)
        {
            _dataService.RemoveOutlier(outlier);
            _outliers.Remove(outlier);
        }

        public void EditOutlier(IOutlier outlier)
        {
            _dataService.EditOutlier(outlier);
            InitializeOutliers();
        }
    }

    public class FilteredData : IFilteredData
    {
        private readonly IDictionary<DateOnly, bool> _filteredDates = new Dictionary<DateOnly, bool>();
        public IDictionary<DateOnly, bool> FilteredDates { get { return _filteredDates; } }

        public bool Contains(DateOnly dateOnly)
        {
            return _filteredDates.ContainsKey(dateOnly);
        }

        public void AddOrUpdate(DateOnly dateOnly, bool included)
        {
			if (_filteredDates.TryGetValue(dateOnly, out _))
            {
                _filteredDates[dateOnly] = included;
            }
            else
                _filteredDates.Add(dateOnly, included);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Merge(IFilteredData filteredData)
        {
            InParameter.NotNull("filteredData", filteredData);
            filteredData.FilteredDates.ForEach(f => AddOrUpdate(f.Key, f.Value));
        }

        public HashSet<DateOnly> FilteredDateList()
        {
            var filteredDates = new HashSet<DateOnly>();
            _filteredDates.ForEach(d => { if (!d.Value) filteredDates.Add(d.Key); });
            return filteredDates;
        }
    }

    public interface IFilteredData
    {
        IDictionary<DateOnly, bool> FilteredDates { get; }
        bool Contains(DateOnly dateOnly);
        void AddOrUpdate(DateOnly dateOnly, bool included);
        void Merge(IFilteredData filteredData);
        HashSet<DateOnly> FilteredDateList();
    }
}
