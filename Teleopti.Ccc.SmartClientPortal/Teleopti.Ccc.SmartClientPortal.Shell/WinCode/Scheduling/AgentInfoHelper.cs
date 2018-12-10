using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public class AgentInfoHelper
	{
		private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
		private readonly IPerson _selectedPerson;
		private readonly ISchedulingResultStateHolder _stateHolder;

		private bool _periodInLegalState;
		private bool _weekInLegalState;
		private TimeSpan _schedulePeriodTargetTime;
		private TimePeriod _schedulePeriodTargetMinMax;
		private int _schedulePeriodTargetDaysOff;
		private int _currentDaysOff;
		private TimeSpan _currentContractTime;
		private TimeSpan _currentWorkTime;
		private TimeSpan _currentPaidTime;
		private int _currentOccupiedSlots;
		private DateOnlyPeriod? _period;
		private readonly IVirtualSchedulePeriod _schedulePeriod;
		private readonly DateOnly _selectedDate;
		private readonly SchedulingOptions _schedulingOptions;
		private readonly IDictionary<string, TimeSpan> _timePerDefinitionSet = new Dictionary<string, TimeSpan>();
		private readonly IScheduleMatrixPro _matrix;
		private readonly IPeriodScheduledAndRestrictionDaysOff _periodScheduledAndRestrictionDaysOff = new PeriodScheduledAndRestrictionDaysOff();
		private string _daysOffTolerance;
		private Percent _preferenceFulfillment;
		private Percent _mustHavesFulfillment;
		private Percent _rotationFulfillment;
		private Percent _availabilityFulfillment;
		private Percent _studentAvailabilityFulfillment;

		public AgentInfoHelper(IPerson person, DateOnly dateOnly, ISchedulingResultStateHolder stateHolder,
			SchedulingOptions schedulingOptions, MatrixListFactory matrixListFactory,
			IWorkShiftMinMaxCalculator workShiftMinMaxCalculator)
		{
			_workShiftMinMaxCalculator = workShiftMinMaxCalculator;
			if (person != null)
			{
				_selectedPerson = person;
				_stateHolder = stateHolder;
				_selectedDate = dateOnly;
				_schedulePeriod = _selectedPerson.VirtualSchedulePeriod(SelectedDate);
				_schedulingOptions = schedulingOptions;

				if (SchedulePeriod.IsValid)
				{
					_period = SchedulePeriod.DateOnlyPeriod;
					DateOnly periodFirstDate = Period.Value.StartDate;
					IScheduleDay onePart = _stateHolder.Schedules[_selectedPerson].ScheduledDay(periodFirstDate);
					_matrix = matrixListFactory.CreateMatrixListForSelection(stateHolder.Schedules, new List <IScheduleDay> {onePart}).FirstOrDefault();
				}

			}
		}

		public bool HasMatrix
		{
			get { return _matrix != null; }
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
				return LanguageResourceHelper.TranslateEnumValue(_schedulePeriod.PeriodType);
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
			get { return _periodScheduledAndRestrictionDaysOff.CalculatedDaysOff(_matrix, IncludeScheduling(), _schedulingOptions.UsePreferences, _schedulingOptions.UseRotations); }
		}

		public SchedulingOptions SchedulingOptions
		{
			get { return _schedulingOptions; }
		}

		public int NumberOfWarnings { get; set; }

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
			get { return new Percent(1 - _mustHavesFulfillment.Value); }
		}

		public void SchedulePeriodData(bool calculateLegalState)
		{
			if (SchedulePeriod == null)
				return;

			if (!SchedulePeriod.IsValid)
				return;

			ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator =
				 new SchedulePeriodTargetTimeCalculator(); //Out

			_schedulePeriodTargetTime = schedulePeriodTargetTimeCalculator.TargetTime(_matrix);
			_schedulePeriodTargetMinMax = schedulePeriodTargetTimeCalculator.TargetWithTolerance(_matrix);
			_schedulePeriodTargetDaysOff = SchedulePeriod.DaysOff();
			_daysOffTolerance = " (" + (_schedulePeriodTargetDaysOff - SchedulePeriod.Contract.NegativeDayOffTolerance) +
									  " - " + (_schedulePeriodTargetDaysOff + SchedulePeriod.Contract.PositiveDayOffTolerance) +
									  ")";

			_currentDaysOff =
					  _periodScheduledAndRestrictionDaysOff.CalculatedDaysOff(_matrix, IncludeScheduling(), false, false);
			setCurrentScheduled();
			setMinMaxData(calculateLegalState);

		}

		private void setMinMaxData(bool calculateLegalState)
		{
			//TODO kan plockas från AutoFac istället
			if (calculateLegalState)
			{
				_periodInLegalState = _workShiftMinMaxCalculator.IsPeriodInLegalState(_matrix, _schedulingOptions);
				_weekInLegalState = _workShiftMinMaxCalculator.IsWeekInLegalState(_selectedDate, _matrix, _schedulingOptions);
			}

		}

		public void SetRestrictionFullfillment()
		{
			var restrictionOverLimitDecider = new RestrictionOverLimitDecider(new RestrictionChecker());
			_preferenceFulfillment = restrictionOverLimitDecider.PreferencesOverLimit(new Percent(1), _matrix).BrokenPercentage;
			_mustHavesFulfillment = restrictionOverLimitDecider.MustHavesOverLimit(new Percent(1), _matrix).BrokenPercentage;
			_rotationFulfillment = restrictionOverLimitDecider.RotationOverLimit(new Percent(1), _matrix).BrokenPercentage;
			_availabilityFulfillment = restrictionOverLimitDecider.AvailabilitiesOverLimit(new Percent(1), _matrix).BrokenPercentage;
			_studentAvailabilityFulfillment = restrictionOverLimitDecider.StudentAvailabilitiesOverLimit(new Percent(1), _matrix).BrokenPercentage;
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
				}

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
