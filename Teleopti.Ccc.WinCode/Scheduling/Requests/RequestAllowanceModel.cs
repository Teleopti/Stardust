using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.WinCode.Budgeting.Models;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Scheduling.Requests
{
    public interface IRequestAllowanceModel
    {
        string WeekName { get; set; }
        DateOnly SelectedDate { get; }
        DateOnlyPeriod VisibleWeek { get; }
        IScenario DefaultScenario { get; }
        bool TotalAllowanceSelected { get; set; }
        bool AllowanceSelected { get; set; }
        IBudgetGroup SelectedBudgetGroup { get; set; }
        HashSet<IAbsence> AbsencesInBudgetGroup { get; }
        IList<IBudgetGroup> BudgetGroups { get; }
        IList<BudgetAbsenceAllowanceDetailModel> VisibleModel { get; }
        void Initialize(IBudgetGroup budgetGroup, DateOnly defaultDate);
        void ReloadModel(DateOnlyPeriod visibleWeek, bool reloadAllowance);
        void MoveToPreviousWeek();
        void MoveToNextWeek();
    }

    public class RequestAllowanceModel : IRequestAllowanceModel
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IBudgetDayRepository _budgetDayRepository;
        private readonly IBudgetGroupRepository _budgetGroupRepository;
        private readonly ICurrentScenario _scenarioRepository;
        private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
        private IList<double> _allowanceCollection;
        private IList<double> _totalAllowanceCollection;
        private IEnumerable<PayloadWorkTime> _usedAbsences;
        private DateOnly _selectedDate;
        private IList<double> _fteCollection;
        
        public RequestAllowanceModel(IUnitOfWorkFactory unitOfWorkFactory,
                                    IBudgetDayRepository budgetDayRepository, 
                                    IBudgetGroupRepository budgetGroupRepository,
                                    ICurrentScenario scenarioRepository,
                                    IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _budgetDayRepository = budgetDayRepository;
            _budgetGroupRepository = budgetGroupRepository;
            _scenarioRepository = scenarioRepository;
            _scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
            VisibleModel = new List<BudgetAbsenceAllowanceDetailModel>();
            BudgetGroups = new List<IBudgetGroup>();
            AbsencesInBudgetGroup = new HashSet<IAbsence>();
        }

        public string WeekName { get; set; }

        public DateOnly SelectedDate { get { return _selectedDate; } }

        public DateOnlyPeriod VisibleWeek { get; private set; }

        public IScenario DefaultScenario { get; private set; }

        public bool TotalAllowanceSelected { get; set; }

        public bool AllowanceSelected { get; set; }

        public IBudgetGroup SelectedBudgetGroup { get; set; }

        public HashSet<IAbsence> AbsencesInBudgetGroup { get; private set; }

        public IList<IBudgetGroup> BudgetGroups { get; private set; }

        public IList<BudgetAbsenceAllowanceDetailModel> VisibleModel { get; private set; }

        public void Initialize(IBudgetGroup budgetGroup, DateOnly defaultDate)
        {
            using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                _selectedDate = defaultDate;
                calculateVisibleWeek();
                loadDefaultScenario();
                loadBudgetGroups();
                initializeSelectedBudgetGroup(budgetGroup);
            }
        }

        private void calculateVisibleWeek()
        {
            VisibleWeek = DateHelper.GetWeekPeriod(SelectedDate, TeleoptiPrincipal.CurrentPrincipal.Regional.Culture);
        }

        private void initializeSelectedBudgetGroup(IBudgetGroup budgetGroup)
        {
            if (budgetGroup != null)
            {
                SelectedBudgetGroup = budgetGroup;
                return;
            }
            SelectedBudgetGroup = BudgetGroups.FirstOrDefault() ?? new EmptyBudgetGroup();
        }

        public void ReloadModel(DateOnlyPeriod visibleWeek, bool reloadAllowance)
        {
            using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                clearModels();
                WeekName = getWeekHeader(SelectedDate, VisibleWeek);
                if (reloadAllowance)
                {
                    loadBudgetData(visibleWeek);
                }
                loadAbsencesInBudgetGroup();
                loadUsedAbsences(visibleWeek);
                loadModels(visibleWeek);
            }
        }

        private void clearModels()
        {
            VisibleModel.Clear();
            AbsencesInBudgetGroup.Clear();
        }

        private void loadBudgetGroups()
        {
            var list = _budgetGroupRepository.LoadAll();

            if (list != null)
            {
                var sorted = list.OrderBy(n2 => n2.Name);
                list = sorted.ToList();

                foreach (var budgetGroup in list)
                {
                    BudgetGroups.Add(budgetGroup);
                }
            }
        }
        
        private void loadModels(DateOnlyPeriod visibleWeek)
        {
            for (var i = 0; i < visibleWeek.DayCount(); i++)
            {
                var currentDate = visibleWeek.StartDate.AddDays(i);
                var absenceDict = new Dictionary<string, double>();
                foreach (var absence in AbsencesInBudgetGroup)
                {
                    var payloadWorkTime = _usedAbsences.FirstOrDefault(ua => ua.PayloadId.Equals(absence.Id.GetValueOrDefault()) && ua.BelongsToDate.Equals(currentDate));
                    if (payloadWorkTime != null)
                    {
                        var usedFTEs =_fteCollection[i] != 0 ? TimeSpan.FromTicks(payloadWorkTime.TotalContractTime).TotalMinutes*1d/TimeDefinition.MinutesPerHour/_fteCollection[i] : 0d;
                        absenceDict.Add(Convert.ToString(absence.Id, CultureInfo.CurrentCulture), usedFTEs);
                    }
                }
                var allowance = AllowanceSelected ? _allowanceCollection[i] : _totalAllowanceCollection[i];
                var usedTotalAbsences = absenceDict.Sum(a => a.Value);
                var absoluteDiff = allowance - usedTotalAbsences;
                var relativeDiff = new Percent(usedTotalAbsences / allowance);
                var headCounts = getHeadCounts(SelectedBudgetGroup, currentDate);
                
                var detailModel = new BudgetAbsenceAllowanceDetailModel
                                      {
                                          Date = new DateDayModel(currentDate),
                                          Allowance = allowance,
                                          UsedAbsencesDictionary = absenceDict,
                                          UsedTotalAbsences = usedTotalAbsences,
                                          AbsoluteDifference = absoluteDiff,
                                          RelativeDifference = relativeDiff,
                                          TotalHeadCounts = headCounts
                                      };
                VisibleModel.Add(detailModel);
            }
        }

        private int getHeadCounts(IBudgetGroup selectedBudgetGroup, DateOnly currentDate)
        {
            // Need to check CurrentDateUTC...
            //var currentDateUtc = TimeZoneHelper.ConvertToUtc(currentDate, selectedBudgetGroup.TimeZone);
            return _scheduleProjectionReadOnlyRepository.GetNumberOfAbsencesPerDayAndBudgetGroup(selectedBudgetGroup.Id.GetValueOrDefault(), currentDate);
        }

        private void loadAbsencesInBudgetGroup()
        {
            if (SelectedBudgetGroup is EmptyBudgetGroup) return;
            if (!LazyLoadingManager.IsInitialized(SelectedBudgetGroup) || !LazyLoadingManager.IsInitialized(SelectedBudgetGroup.CustomShrinkages))
                _unitOfWorkFactory.CurrentUnitOfWork().Reassociate(SelectedBudgetGroup);
            foreach (var budgetAbsence in SelectedBudgetGroup.CustomShrinkages.SelectMany(customShrinkage => customShrinkage.BudgetAbsenceCollection))
            {
                AbsencesInBudgetGroup.Add(budgetAbsence);
            }
        }

        private void loadBudgetData(DateOnlyPeriod period)
        {
            var budgetDays = new List<IBudgetDay>();
             if (!(SelectedBudgetGroup is EmptyBudgetGroup))
                budgetDays = _budgetDayRepository.Find(DefaultScenario, SelectedBudgetGroup, period).ToList();
            budgetDays = addMissingDays(budgetDays, VisibleWeek).ToList();
            _allowanceCollection = budgetDays.Select(b => b.Allowance).ToList();
            _totalAllowanceCollection = budgetDays.Select(b => b.TotalAllowance).ToList();
            _fteCollection = budgetDays.Select(b => b.FulltimeEquivalentHours).ToList();
        }

        private void loadUsedAbsences(DateOnlyPeriod period)
        {
            if (SelectedBudgetGroup is EmptyBudgetGroup) return;
            _usedAbsences = _scheduleProjectionReadOnlyRepository.AbsenceTimePerBudgetGroup(period, SelectedBudgetGroup, DefaultScenario);
        }

        private void loadDefaultScenario()
        {
            DefaultScenario = _scenarioRepository.Current();
        }

        private IEnumerable<IBudgetDay> addMissingDays(IEnumerable<IBudgetDay> existingBudgetDays, DateOnlyPeriod period)
        {
            var dayCollection = period.DayCollection();
            var allBudgetDaysForPeriod = new List<IBudgetDay>(dayCollection.Count);

            foreach (DateOnly date in dayCollection)
            {
                var budgetDay = existingBudgetDays.FirstOrDefault(d => d.Day == date);
                if (budgetDay == null)
                {
                    budgetDay = new BudgetDay(SelectedBudgetGroup, DefaultScenario, date);
                    initiateBudgetDayWithDefaultValue(budgetDay);
                }
                allBudgetDaysForPeriod.Add(budgetDay);
            }
            return allBudgetDaysForPeriod;
        }

        private static void initiateBudgetDayWithDefaultValue(IBudgetDay budgetDay)
        {
            budgetDay.AbsenceThreshold = new Percent(1.0);
        }
        
        private static string getWeekHeader(DateOnly date, DateOnlyPeriod week)
        {
            var weekNumber = DateHelper.WeekNumber(date.Date, CultureInfo.CurrentCulture);
	        return string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.WeekAbbreviationDot, weekNumber,
		        week.StartDate.ToShortDateString());
        }

        public void MoveToPreviousWeek()
        {
            _selectedDate = _selectedDate.AddDays(-7);
            calculateVisibleWeek();
        }

        public void MoveToNextWeek()
        {
            _selectedDate = _selectedDate.AddDays(7);
            calculateVisibleWeek();
        }
    }

    public class EmptyBudgetGroup : BudgetGroup
    {
        public override string Name
        {
            get { return Name = UserTexts.Resources.Empty; }
        }
    }
}