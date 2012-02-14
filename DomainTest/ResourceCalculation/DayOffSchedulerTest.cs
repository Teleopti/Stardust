using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class DayOffSchedulerTest
	{
		private MockRepository _mocks;

		private  ISchedulingResultStateHolder _schedulingResultStateHolder;
		private  IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		private  IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private  ISchedulingOptions _schedulingOptions;
		private  ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private IEffectiveRestriction _effectiveRestriction;
		private IDayOffScheduler _target;
		private DateOnly _date1 = new DateOnly(2009, 2, 2);
		private IScheduleDay _part1;
		private IPerson _person;
		private IScheduleDictionary _schedules;
		private IScheduleRange _range;
		private IScheduleDayAvailableForDayOffSpecification _scheduleAvailableForDayOffSpecification;
	    private IScheduleMatrixListCreator _scheduleMatrixListCreator;
	    private IScheduleMatrixPro _matrix;
	    private IVirtualSchedulePeriod _schedulePeriod;
	    private IContract _contract;
        private DateOnlyPeriod _period;
	    private IScheduleDayPro _scheduleDayPro;
	    private IContractSchedule _contractSchedule;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
            _period = new DateOnlyPeriod(_date1, _date1.AddDays(1));
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_dayOffsInPeriodCalculator = _mocks.StrictMock<IDayOffsInPeriodCalculator>();
			_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
			_effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
																				  new EndTimeLimitation(),
																				  new WorkTimeLimitation()
																				  , null, null, null,
																				  new List<IActivityRestriction>());
			_schedulingOptions = new SchedulingOptions();
			_schedulePartModifyAndRollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_scheduleAvailableForDayOffSpecification = _mocks.StrictMock<IScheduleDayAvailableForDayOffSpecification>();
		    _scheduleMatrixListCreator = _mocks.StrictMock<IScheduleMatrixListCreator>();
			_target = new DayOffScheduler(_schedulingResultStateHolder, _dayOffsInPeriodCalculator, _effectiveRestrictionCreator,
                                          _schedulingOptions, _schedulePartModifyAndRollbackService, _scheduleAvailableForDayOffSpecification, _scheduleMatrixListCreator);

			_schedules = _mocks.StrictMock<IScheduleDictionary>();
			_range = _mocks.StrictMock<IScheduleRange>();
			_part1 = _mocks.StrictMock<IScheduleDay>();
			_person = _mocks.StrictMock<IPerson>();
		    _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
		    _schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
		    _contract = _mocks.StrictMock<IContract>();
            _schedulingOptions.DayOffTemplate = new DayOffTemplate(new Description("hej"));
		    _scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            _contractSchedule = _mocks.StrictMock<IContractSchedule>();
		}

        [Test]
        public void AddPreferenceDayOffShouldWork()
        {
            _effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;

            using (_mocks.Record())
            {
                commonMocks();
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(_schedules).Repeat.AtLeastOnce();
                Expect.Call(_schedules[_person]).Return(_range).Repeat.AtLeastOnce();
                Expect.Call(_range.ScheduledDay(_date1)).Return(_part1).Repeat.AtLeastOnce();
                Expect.Call(_part1.IsScheduled()).Return(false).Repeat.Any();
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_part1, _schedulingOptions)).Return(_effectiveRestriction);
                Expect.Call(() => _part1.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
                Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_part1));

                mocksForNotAddingContractDaysoff();
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(new List<IScheduleDay> { _part1 }, new List<DateOnly> { _date1 }, new List<IPerson> { _person }, _schedulePartModifyAndRollbackService);
            }
        }

        [Test]
        public void AddPreferenceDayOffShouldRollbackIfOnOutsideScheduleException()
        {
            _effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;

            using (_mocks.Record())
            {
                commonMocks();
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(_schedules).Repeat.AtLeastOnce();
                Expect.Call(_schedules[_person]).Return(_range).Repeat.AtLeastOnce();
                Expect.Call(_range.ScheduledDay(_date1)).Return(_part1).Repeat.AtLeastOnce();
                Expect.Call(_part1.IsScheduled()).Return(false).Repeat.Any();
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_part1, _schedulingOptions)).Return(_effectiveRestriction);
                Expect.Call(() => _part1.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
                Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_part1)).Throw(new DayOffOutsideScheduleException());
                Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback());

                mocksForNotAddingContractDaysoff();
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(new List<IScheduleDay> { _part1 }, new List<DateOnly> { _date1 }, new List<IPerson> { _person }, _schedulePartModifyAndRollbackService);
            }
        }

        [Test]
        public void AddPreferenceDayOffShouldNotAddIfConflict()
        {
            _effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;
            _schedulingOptions.RotationDaysOnly = true;

            using (_mocks.Record())
            {
                commonMocks();
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(_schedules).Repeat.AtLeastOnce();
                Expect.Call(_schedules[_person]).Return(_range).Repeat.AtLeastOnce();
                Expect.Call(_range.ScheduledDay(_date1)).Return(_part1).Repeat.AtLeastOnce();
                Expect.Call(_part1.IsScheduled()).Return(false).Repeat.Any();
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_part1, _schedulingOptions)).Return(_effectiveRestriction);

                mocksForNotAddingContractDaysoff();
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(new List<IScheduleDay> { _part1 }, new List<DateOnly> { _date1 }, new List<IPerson> { _person }, _schedulePartModifyAndRollbackService);
            }
        }

        [Test]
        public void AddPreferenceDayOffShouldStopIfCanceled()
        {
            _effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;

            using (_mocks.Record())
            {
                commonMocks();
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(_schedules).Repeat.AtLeastOnce();
                Expect.Call(_schedules[_person]).Return(_range).Repeat.AtLeastOnce();
                Expect.Call(_range.ScheduledDay(_date1)).Return(_part1).Repeat.AtLeastOnce();
                Expect.Call(_part1.IsScheduled()).Return(false).Repeat.Any();
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_part1, _schedulingOptions)).Return(_effectiveRestriction);
                Expect.Call(() => _part1.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
                Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_part1));

                mocksForNotAddingContractDaysoff();
            }

            using (_mocks.Playback())
            {
                _target.DayScheduled += _target_DayScheduled;
                _target.DayOffScheduling(new List<IScheduleDay> { _part1 }, new List<DateOnly> { _date1 }, new List<IPerson> { _person }, _schedulePartModifyAndRollbackService);
                _target.DayScheduled += _target_DayScheduled;
            }
        }

		[Test]
		public void AddContractDayOffShouldDoNothingIfDayIsScheduled()
		{
            using (_mocks.Record())
            {
                commonMocks();
                mocksForNotAddingPreferences();

                Expect.Call(_schedulePeriod.IsValid).Return(true);
                Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
                int x;
                int y;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(1, 0);
                Expect.Call(_scheduleAvailableForDayOffSpecification.IsSatisfiedBy(_part1)).Return(false);
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(new List<IScheduleDay> { _part1 }, new List<DateOnly> { _date1 }, new List<IPerson> { _person }, _schedulePartModifyAndRollbackService);
            }
		}

		[Test ,ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowIfRollbackServiceIsNull()
		{
			Expect.Call(_schedulingResultStateHolder.Schedules).Return(_schedules).Repeat.AtLeastOnce();
			Expect.Call(_schedules[_person]).Return(_range).Repeat.AtLeastOnce();
			Expect.Call(_range.ScheduledDay(_date1)).Return(_part1).Repeat.AtLeastOnce();
			Expect.Call(_part1.IsScheduled()).Return(true).Repeat.Any();
			_mocks.ReplayAll();
            _target.DayOffScheduling(new List<IScheduleDay> { _part1 }, new List<DateOnly> { _date1 }, new List<IPerson> { _person }, null);
			_mocks.VerifyAll();
		}

		[Test]
		public void VerifyAddContractDaysOff()
		{
			using (_mocks.Record())
			{
			    commonMocks();
			    mocksForNotAddingPreferences();

			    Expect.Call(_schedulePeriod.IsValid).Return(true);
                Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
			    int x;
			    int y;
			    Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(1, 0);
                Expect.Call(_scheduleAvailableForDayOffSpecification.IsSatisfiedBy(_part1)).Return(true);
			    Expect.Call(_contractSchedule.IsWorkday(_period.StartDate, _date1)).Return(false);
			    Expect.Call(() => _part1.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
                Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_part1));
			}

			using (_mocks.Playback())
			{
                _target.DayOffScheduling(new List<IScheduleDay> { _part1 }, new List<DateOnly> { _date1 }, new List<IPerson> { _person }, _schedulePartModifyAndRollbackService);
			}
		}

		[Test]
		public void ShouldRollbackOnOutsideScheduleExceptionOnAddContractsDayOff()
		{
			using (_mocks.Record())
			{
                commonMocks();
                mocksForNotAddingPreferences();

                Expect.Call(_schedulePeriod.IsValid).Return(true);
                Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
                int x;
                int y;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(1, 0);
                Expect.Call(_scheduleAvailableForDayOffSpecification.IsSatisfiedBy(_part1)).Return(true);
                Expect.Call(_contractSchedule.IsWorkday(_period.StartDate, _date1)).Return(false);
                Expect.Call(() => _part1.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
                Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_part1)).Throw(new DayOffOutsideScheduleException());
                Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback());
			}

			using (_mocks.Playback())
			{
                _target.DayOffScheduling(new List<IScheduleDay> { _part1 }, new List<DateOnly> { _date1 }, new List<IPerson> { _person }, _schedulePartModifyAndRollbackService);
			}
		}

		[Test]
		public void ShouldJumpOutIfCanceledInAddContractDaysOff()
		{
			var date2 = new DateOnly(2009, 2, 3);

			using (_mocks.Record())
			{
                commonMocks();
                mocksForNotAddingPreferences();
			    Expect.Call(_range.ScheduledDay(date2)).Return(_part1);

                Expect.Call(_schedulePeriod.IsValid).Return(true);
                Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
                int x;
                int y;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(1, 0);
                Expect.Call(_scheduleAvailableForDayOffSpecification.IsSatisfiedBy(_part1)).Return(true);
                Expect.Call(_contractSchedule.IsWorkday(_period.StartDate, _date1)).Return(false);
                Expect.Call(() => _part1.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
                Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_part1));
			}

			using (_mocks.Playback())
			{
                _target.DayScheduled += _target_DayScheduled;
                _target.DayOffScheduling(new List<IScheduleDay> { _part1 }, new List<DateOnly> { _date1, date2 }, new List<IPerson> { _person }, _schedulePartModifyAndRollbackService);
                _target.DayScheduled -= _target_DayScheduled;
			}
		}

        [Test]
        public void ShouldJumpOutOfContractDaysOffIfSchedulePeriodNotValid()
        {
            using (_mocks.Record())
            {
                commonMocks();
                mocksForNotAddingPreferences();
                mocksForNotAddingContractDaysoff();
            }

            using (_mocks.Playback())
			{
                _target.DayOffScheduling(new List<IScheduleDay> { _part1 }, new List<DateOnly> { _date1 }, new List<IPerson> { _person }, _schedulePartModifyAndRollbackService);
			}
        }

        [Test]
        public void ShouldJumpOutOfContractDaysOffIfEmploymentTypeIsHourlyStaff()
        {
            using (_mocks.Record())
            {
                commonMocks();
                mocksForNotAddingPreferences();

                Expect.Call(_schedulePeriod.IsValid).Return(true);
                Expect.Call(_contract.EmploymentType).Return(EmploymentType.HourlyStaff);
            }

            using (_mocks.Playback())
			{
                _target.DayOffScheduling(new List<IScheduleDay> { _part1 }, new List<DateOnly> { _date1 }, new List<IPerson> { _person }, _schedulePartModifyAndRollbackService);
			}
        }

        [Test]
        public void ShouldJumpOutOfContractDaysOffIfHasCorrectNumberOfDaysOffAndDaysOffIsNotZero()
        {
            using (_mocks.Record())
            {
                commonMocks();
                mocksForNotAddingPreferences();

                Expect.Call(_schedulePeriod.IsValid).Return(true);
                Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
                int x;
                int y;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(1, 1);
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(new List<IScheduleDay> { _part1 }, new List<DateOnly> { _date1 }, new List<IPerson> { _person }, _schedulePartModifyAndRollbackService);
            }
        }

        [Test]
        public void ShouldNotAddContractDayOffWhenAllHaveBeenAdded()
        {
            using (_mocks.Record())
            {
                commonMocks();
                mocksForNotAddingPreferences();

                Expect.Call(_schedulePeriod.IsValid).Return(true);
                Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
                int x;
                int y;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(0, 0);
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(new List<IScheduleDay> { _part1 }, new List<DateOnly> { _date1 }, new List<IPerson> { _person }, _schedulePartModifyAndRollbackService);
            }
        }

        [Test]
        public void ShouldNotAddContractDayOffWhenContractWorkday()
        {
            using (_mocks.Record())
            {
                commonMocks();
                mocksForNotAddingPreferences();

                Expect.Call(_schedulePeriod.IsValid).Return(true);
                Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
                int x;
                int y;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(1, 0);
                Expect.Call(_scheduleAvailableForDayOffSpecification.IsSatisfiedBy(_part1)).Return(true);
                Expect.Call(_contractSchedule.IsWorkday(_period.StartDate, _date1)).Return(true);
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(new List<IScheduleDay> { _part1 }, new List<DateOnly> { _date1 }, new List<IPerson> { _person }, _schedulePartModifyAndRollbackService);
            }
        }

        void _target_DayScheduled(object sender, SchedulingServiceBaseEventArgs e)
        {
            e.Cancel = true;
        }

        private void commonMocks()
        {
            Expect.Call(_scheduleMatrixListCreator.CreateMatrixListFromScheduleParts(new List<IScheduleDay> {_part1})).
                Return(new List<IScheduleMatrixPro> {_matrix});
            Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
            Expect.Call(_schedulePeriod.Contract).Return(_contract).Repeat.Any();
            Expect.Call(() => _matrix.UnlockPeriod(_period)).Repeat.Any();
            Expect.Call(_matrix.UnlockedDays).Return(
                new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {_scheduleDayPro})).Repeat.Any();
            Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(_period).Repeat.Any();
            Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_part1).Repeat.Any();
            Expect.Call(_schedulePeriod.ContractSchedule).Return(_contractSchedule).Repeat.Any();
            Expect.Call(_scheduleDayPro.Day).Return(_date1).Repeat.Any();
        }

        private void mocksForNotAddingPreferences()
        {
            Expect.Call(_schedulingResultStateHolder.Schedules).Return(_schedules).Repeat.AtLeastOnce();
            Expect.Call(_schedules[_person]).Return(_range).Repeat.AtLeastOnce();
            Expect.Call(_range.ScheduledDay(_date1)).Return(_part1).Repeat.AtLeastOnce();
            Expect.Call(_part1.IsScheduled()).Return(true).Repeat.Any();
        }

        private void mocksForNotAddingContractDaysoff()
        {
            Expect.Call(_schedulePeriod.IsValid).Return(false);
        }
	}
}