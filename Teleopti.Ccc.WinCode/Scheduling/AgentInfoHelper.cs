using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public class AgentInfoHelper
    {
        private readonly IRuleSetProjectionService _ruleSetProjectionService;
        private IPerson _selectedPerson;
        private ISchedulingResultStateHolder _stateHolder;

        private bool _periodInLegalState;
        private bool _weekInLegalState;
        private TimeSpan _schedulePeriodTargetTime;
        private TimePeriod _schedulePeriodTargetMinMax;
        private int _schedulePeriodTargetDaysOff;
        private int _currentDaysOff;
        private TimeSpan _currentContractTime;
        private TimeSpan _currentWorkTime;
        private TimeSpan _currentPaidTime;
        //private TimeSpan _currentOvertime;
        //private TimeSpan _currentShiftAllowanceTime;
        private int _currentOccupiedSlots;
        private DateOnlyPeriod? _period;
        private IVirtualSchedulePeriod _schedulePeriod;
        private DateOnly _selectedDate;
        private ISchedulingOptions _schedulingOptions;
        private MinMax<TimeSpan> _possiblePeriodTime;
        private int _numberOfWarnings;
        private IDictionary<string, TimeSpan> _timePerDefinitionSet = new Dictionary<string, TimeSpan>();
        private IScheduleMatrixPro _matrix;
        private IPeriodScheduledAndRestrictionDaysOff _periodScheduledAndRestrictionDaysOff = new PeriodScheduledAndRestrictionDaysOff();
        private string _daysOffTolerance;
        private Percent _preferenceFulfillment;
        private Percent _mustHavesFulfillment;
        private Percent _rotationFulfillment;
        private Percent _availabilityFulfillment;
        private Percent _studentAvailabilityFulfillment;

        public AgentInfoHelper(IPerson person, DateOnly dateOnly, ISchedulingResultStateHolder stateHolder, ISchedulingOptions schedulingOptions, IRuleSetProjectionService ruleSetProjectionService)
        {
            _ruleSetProjectionService = ruleSetProjectionService;
            if (person != null)
            {
                _selectedPerson = person;
                _stateHolder = stateHolder;
                _selectedDate = dateOnly;
                _schedulePeriod = _selectedPerson.VirtualSchedulePeriod(SelectedDate);
                _schedulingOptions = schedulingOptions;
                _period = SchedulePeriod.DateOnlyPeriod;

                if(SchedulePeriod.IsValid)
                {
                    DateOnly periodFirstDate = Period.Value.StartDate;
                    IScheduleDay onePart = _stateHolder.Schedules[_selectedPerson].ScheduledDay(periodFirstDate);
                    IScheduleMatrixListCreator scheduleMatrixListCreator = new ScheduleMatrixListCreator(_stateHolder);
                    _matrix = scheduleMatrixListCreator.CreateMatrixListFromScheduleParts(new List<IScheduleDay> { onePart })[0];
                }
               
            }
        }

        public bool PeriodInLegalState
        {
            get { return _periodInLegalState; }
        }

        public bool WeekInLegalState
        {
            get { return _weekInLegalState; }
        }

        public TimeSpan SchedulePeriodTargetTime
        {
            get { return _schedulePeriodTargetTime; }
        }

        public TimePeriod SchedulePeriodTargetMinMax
        {
            get { return _schedulePeriodTargetMinMax; }
        }

        public int SchedulePeriodTargetDaysOff
        {
            get { return _schedulePeriodTargetDaysOff; }
        }

        public int CurrentDaysOff
        {
            get { return _currentDaysOff; }
        }

        public TimeSpan CurrentContractTime
        {
            get { return _currentContractTime; }
        }

        public TimeSpan CurrentWorkTime
        {
            get { return _currentWorkTime; }
        }

        public TimeSpan CurrentPaidTime
        {
            get { return _currentPaidTime; }
        }

        //public TimeSpan CurrentOvertime
        //{
        //    get { return _currentOvertime; }
        //}

        public int CurrentOccupiedSlots
        {
            get { return _currentOccupiedSlots; }
        }

        public DateOnlyPeriod? Period
        {
            get { return _period; }
        }

        public IVirtualSchedulePeriod SchedulePeriod
        {
            get { return _schedulePeriod; }
        }

        public IPerson Person
        {
            get { return _selectedPerson; }
        }
        public string PersonName
        {
            get { return _selectedPerson.Name.ToString(); }
        }
        public string PeriodType
        {
            get
            {
                if (_schedulePeriod.PeriodType == SchedulePeriodType.Week)
                {
                    return UserTexts.Resources.Week;
                }
                if (_schedulePeriod.PeriodType == SchedulePeriodType.Day)
                {
                    return UserTexts.Resources.Day;
                }
                return UserTexts.Resources.Month;
            }
        }
        public DateOnly SelectedDate
        {
            get { return _selectedDate; }
        }
        public DateOnly StartDate
        {
            get { return _period.Value.StartDate; }
        }
        public DateOnly EndDate
        {
            get { return _period.Value.EndDate; }
        }

        public int NumberOfDatesWithPreferenceOrScheduledDaysOff
        {
            get { return _periodScheduledAndRestrictionDaysOff.CalculatedDaysOff(new RestrictionExtractor(_stateHolder), _matrix, IncludeScheduling(), _schedulingOptions.UsePreferences, _schedulingOptions.UseRotations); }
        }

        public MinMax<TimeSpan> PossiblePeriodTime
        {
            get { return _possiblePeriodTime; }
        }

        public TimeSpan MinPossiblePeriodTime
        {
            get { return _possiblePeriodTime.Minimum; }
        }

        public TimeSpan MaxPossiblePeriodTime
        {
            get { return _possiblePeriodTime.Maximum; }
        }

        public ISchedulingOptions SchedulingOptions
        {
            get { return _schedulingOptions; }
        }

        public int NumberOfWarnings
        {
            get { return _numberOfWarnings; }
            set { _numberOfWarnings = value; }
        }

        public IDictionary<string, TimeSpan> TimePerDefinitionSet
        {
            get { return _timePerDefinitionSet; }
        }

        public string DayOffTolerance
        {
            get { return _daysOffTolerance; }
        }

        public Percent PreferenceFulfillment
        {
            get { return new Percent(1 - _preferenceFulfillment.Value); }
        }

        public Percent RotationFulfillment
        {
            get { return new Percent(1 - _rotationFulfillment.Value); }
        }

        public Percent AvailabilityFulfillment
        {
            get { return new Percent(1 - _availabilityFulfillment.Value); }
        }

        public Percent StudentAvailabilityFulfillment
        {
            get { return new Percent(1 - _studentAvailabilityFulfillment.Value); }
        }

        public Percent MustHavesFulfillment
        {
            get { return new Percent(1 -  _mustHavesFulfillment.Value); }
        }

        //public TimeSpan CurrentShiftAllowanceTime
        //{
        //    get { return _currentShiftAllowanceTime; }
        //}

        public void SchedulePeriodData()
        {
            if (SchedulePeriod == null)
                return;

            if (!SchedulePeriod.IsValid)
                return;

            ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator =
                new SchedulePeriodTargetTimeTimeCalculator(); //Out

            _schedulePeriodTargetTime = schedulePeriodTargetTimeCalculator.TargetTime(_matrix);
            _schedulePeriodTargetMinMax = schedulePeriodTargetTimeCalculator.TargetWithTolerance(_matrix);
            _schedulePeriodTargetDaysOff = SchedulePeriod.DaysOff();
            _daysOffTolerance = " (" + (_schedulePeriodTargetDaysOff - SchedulePeriod.Contract.NegativeDayOffTolerance) +
                                " - " + (_schedulePeriodTargetDaysOff + SchedulePeriod.Contract.PositiveDayOffTolerance) +
                                ")";

            _currentDaysOff =
                    _periodScheduledAndRestrictionDaysOff.CalculatedDaysOff(new RestrictionExtractor(_stateHolder),
                                                                            _matrix, IncludeScheduling(), false, false);
            setCurrentScheduled();
            setMinMaxData();
            setRestrictionFullfillment();
        }

        private void setMinMaxData()
        {
            ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator =
                       new SchedulePeriodTargetTimeTimeCalculator(); //Out

            IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_stateHolder);

            IPossibleMinMaxWorkShiftLengthExtractor possibleMinMaxWorkShiftLengthExtractor =
                new PossibleMinMaxWorkShiftLengthExtractor(restrictionExtractor, _ruleSetProjectionService);

            IWorkShiftWeekMinMaxCalculator workShiftWeekMinMaxCalculator = new WorkShiftWeekMinMaxCalculator();
            IWorkShiftMinMaxCalculator workShiftMinMaxCalculator =
                new WorkShiftMinMaxCalculator(possibleMinMaxWorkShiftLengthExtractor,
                                              schedulePeriodTargetTimeCalculator, workShiftWeekMinMaxCalculator);
            //TODO kan plockas från AutoFac istället
            _periodInLegalState = workShiftMinMaxCalculator.IsPeriodInLegalState(_matrix, _schedulingOptions);
            _weekInLegalState = workShiftMinMaxCalculator.IsWeekInLegalState(_selectedDate, _matrix, _schedulingOptions);


            _possiblePeriodTime = workShiftMinMaxCalculator.PossibleMinMaxTimeForPeriod(_matrix, _schedulingOptions);
        }

        private void setRestrictionFullfillment()
        {
            RestrictionOverLimitDecider restrictionOverLimitDecider = new RestrictionOverLimitDecider(_matrix, new RestrictionChecker());
            _preferenceFulfillment = restrictionOverLimitDecider.PreferencesOverLimit(new Percent(1)).BrokenPercentage;
            _mustHavesFulfillment = restrictionOverLimitDecider.MustHavesOverLimit(new Percent(1)).BrokenPercentage;
            _rotationFulfillment = restrictionOverLimitDecider.RotationOverLimit(new Percent(1)).BrokenPercentage;
            _availabilityFulfillment = restrictionOverLimitDecider.AvailabilitiesOverLimit(new Percent(1)).BrokenPercentage;
            _studentAvailabilityFulfillment = restrictionOverLimitDecider.StudentAvailabilitiesOverLimit(new Percent(1)).BrokenPercentage;
        }

        private void setCurrentScheduled()
        {
            resetCurrentTimes();

            foreach (var localDate in Period.Value.DayCollection())
            {
                IScheduleDay schedulePart = _stateHolder.Schedules[_selectedPerson].ScheduledDay(localDate);
                IProjectionService projSvc = schedulePart.ProjectionService();
                IVisualLayerCollection res = projSvc.CreateProjection();

                if (IncludeScheduling())
                    _currentContractTime = CurrentContractTime.Add(res.ContractTime());
                _currentWorkTime = CurrentWorkTime.Add(res.WorkTime());
                _currentPaidTime = CurrentPaidTime.Add(res.PaidTime());
                //OvertimeAndShiftAllowances
                IMultiplicatorProjectionService multiplicatorProjectionService = new MultiplicatorProjectionService(schedulePart, localDate);
                IList<IMultiplicatorLayer> overtimeAndShiftAllowanceList = multiplicatorProjectionService.CreateProjection();
                foreach (var multiplicatorLayer in overtimeAndShiftAllowanceList)
                {
                    string key = multiplicatorLayer.MultiplicatorDefinitionSet.Name;
                    if (!_timePerDefinitionSet.ContainsKey(key))
                        _timePerDefinitionSet.Add(multiplicatorLayer.MultiplicatorDefinitionSet.Name, TimeSpan.Zero);
                    _timePerDefinitionSet[key] = _timePerDefinitionSet[key].Add(multiplicatorLayer.Period.ElapsedTime());
                    //if(multiplicatorLayer.MultiplicatorDefinitionSet.MultiplicatorType == MultiplicatorType.Overtime)
                    //    _currentOvertime = CurrentOvertime.Add(multiplicatorLayer.Period.ElapsedTime());
                    //if (multiplicatorLayer.MultiplicatorDefinitionSet.MultiplicatorType == MultiplicatorType.OBTime)
                    //    _currentShiftAllowanceTime = CurrentShiftAllowanceTime.Add(multiplicatorLayer.Period.ElapsedTime());
                }
                //IDictionary<string, TimeSpan> timePerDefToday = res.TimePerDefinitionSet();
                //foreach (var multiplicatorDefinitionSetName in timePerDefToday.Keys)
                //{
                //    if (!_timePerDefinitionSet.ContainsKey(multiplicatorDefinitionSetName))
                //        _timePerDefinitionSet.Add(multiplicatorDefinitionSetName, TimeSpan.Zero);

                //    _timePerDefinitionSet[multiplicatorDefinitionSetName] =
                //        _timePerDefinitionSet[multiplicatorDefinitionSetName].Add(
                //            timePerDefToday[multiplicatorDefinitionSetName]);

                //}

                if (schedulePart.IsScheduled())
                    _currentOccupiedSlots = _currentOccupiedSlots + 1;
            }
        }

        private void resetCurrentTimes()
        {
            _currentContractTime = TimeSpan.Zero;
            _currentWorkTime = TimeSpan.Zero;
            _currentPaidTime = TimeSpan.Zero;
            _currentOccupiedSlots = 0;
        }

        public bool IncludeScheduling()
        {
            if (_schedulingOptions.GetType() != typeof(RestrictionSchedulingOptions) || ((RestrictionSchedulingOptions)_schedulingOptions).UseScheduling)
                return true;
            return false;
        }
    }
}
