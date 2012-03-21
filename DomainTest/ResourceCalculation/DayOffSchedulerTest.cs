using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
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
		private IScheduleDayAvailableForDayOffSpecification _scheduleAvailableForDayOffSpecification;
	    private IScheduleMatrixListCreator _scheduleMatrixListCreator;
	    private IVirtualSchedulePeriod _schedulePeriod;
	    private IContract _contract;
        private DateOnlyPeriod _period;
	    private IScheduleDayPro _scheduleDayPro;
	    private IContractSchedule _contractSchedule;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private IScheduleDay _scheduleDay;
	    private IScheduleDayPro _scheduleDayPro2;

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

           
		    _schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
		    _contract = _mocks.StrictMock<IContract>();
            _schedulingOptions.DayOffTemplate = new DayOffTemplate(new Description("hej"));
		    _scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            _contractSchedule = _mocks.StrictMock<IContractSchedule>();
            _scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
            _scheduleDay = _mocks.StrictMock<IScheduleDay>();
            _scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
		}

        [Test]
        public void ShouldAddPreferenceDayOff()
        {
            _effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
       
            using(_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList);
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false).Repeat.Any();
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _schedulingOptions)).Return(_effectiveRestriction);
                Expect.Call(() => _scheduleDay.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
                Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay));

                MocksForNotAddingContractDaysoff();
            }

            using(_mocks.Playback())
            {
                _target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService);   
            }
        }

        [Test]
        public void ShouldRollbackIfOnOutsideScheduleExceptionWhenAddPrefrenceDayOff()
        {
            _effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });

            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList);
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false).Repeat.Any();
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _schedulingOptions)).Return(_effectiveRestriction);
                Expect.Call(() => _scheduleDay.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
                Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay)).Throw(new DayOffOutsideScheduleException());
                Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback());

                MocksForNotAddingContractDaysoff();
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService);
            }   
        }

        [Test]
        public void ShouldNotAddPreferenceDayOffIfConflict()
        {
            _effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;
            _schedulingOptions.RotationDaysOnly = true;
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });

            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList);
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false).Repeat.Any();
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _schedulingOptions)).Return(_effectiveRestriction);
               
                MocksForNotAddingContractDaysoff();
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService);
            }    
        }

        [Test]
        public void ShouldStopAddingPreferenceDayOffIfCanceled()
        {
            _effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });

            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList);
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false).Repeat.Any();
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _schedulingOptions)).Return(_effectiveRestriction);
                Expect.Call(() => _scheduleDay.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
                Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay));

                MocksForNotAddingContractDaysoff();
            }

            using (_mocks.Playback())
            {
                _target.DayScheduled += _target_DayScheduled;
                _target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService);
                _target.DayScheduled += _target_DayScheduled;
            }        
        }

        [Test]
        public void ShouldNotAddPreferenceDayOffOnScheduledDay()
        {
            _effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });

            using (_mocks.Record())
            {
                Expect.Call(_schedulePeriod.IsValid).Return(true);
                Expect.Call(_schedulePeriod.Contract).Return(_contract);
                Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.IsScheduled()).Return(true).Repeat.Any();
                int x;
                int y;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(1, 0);
                Expect.Call(_scheduleAvailableForDayOffSpecification.IsSatisfiedBy(_scheduleDay)).Return(false);
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService);
            }       
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfRollbackServiceIsNull()
        {
            _effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;
            _schedulingOptions.RotationDaysOnly = true;
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });

            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList);
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(true).Repeat.Any();

                MocksForNotAddingContractDaysoff();
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(matrixProList, matrixProList, null);
            }    
        }

        [Test]
        public void ShouldAddContractDayOffs()
        {
            _effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });

            using (_mocks.Record())
            {
                Expect.Call(_schedulePeriod.IsValid).Return(true);
                Expect.Call(_schedulePeriod.Contract).Return(_contract);
                Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.IsScheduled()).Return(true).Repeat.Any();
                int x;
                int y;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(1, 0);
                Expect.Call(_scheduleAvailableForDayOffSpecification.IsSatisfiedBy(_scheduleDay)).Return(true);
                Expect.Call(_schedulePeriod.ContractSchedule).Return(_contractSchedule);
                Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(_period).Repeat.Any();
                Expect.Call(_scheduleDayPro.Day).Return(_date1).Repeat.Any();
                Expect.Call(_contractSchedule.IsWorkday(_period.StartDate, _date1)).Return(false);
                Expect.Call(() => _scheduleDay.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
                Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay));
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService);
            }           
        }

        [Test]
        public void ShouldRollbackOnOutsideScheduleExceptionOnAddContractsDayOff()
        {
            _effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });

            using (_mocks.Record())
            {
                Expect.Call(_schedulePeriod.IsValid).Return(true);
                Expect.Call(_schedulePeriod.Contract).Return(_contract);
                Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.IsScheduled()).Return(true).Repeat.Any();
                int x;
                int y;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(1, 0);
                Expect.Call(_scheduleAvailableForDayOffSpecification.IsSatisfiedBy(_scheduleDay)).Return(true);
                Expect.Call(_schedulePeriod.ContractSchedule).Return(_contractSchedule);
                Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(_period).Repeat.Any();
                Expect.Call(_scheduleDayPro.Day).Return(_date1).Repeat.Any();
                Expect.Call(_contractSchedule.IsWorkday(_period.StartDate, _date1)).Return(false);
                Expect.Call(() => _scheduleDay.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
                Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay)).Throw(new DayOffOutsideScheduleException());
                Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback());
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService);
            }       
        }

        [Test]
        public void ShouldJumpOutIfCanceledInAddContractDaysOff()
        {
            _effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;
            var matrixProList = new List<IScheduleMatrixPro> {_scheduleMatrixPro};
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {_scheduleDayPro, _scheduleDayPro2});

            using (_mocks.Record())
            {
                Expect.Call(_schedulePeriod.IsValid).Return(true);
                Expect.Call(_schedulePeriod.Contract).Return(_contract);
                Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.IsScheduled()).Return(true).Repeat.Any();
                int x;
                int y;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(
                    true).OutRef(1, 0);
                Expect.Call(_scheduleAvailableForDayOffSpecification.IsSatisfiedBy(_scheduleDay)).Return(true);
                Expect.Call(_schedulePeriod.ContractSchedule).Return(_contractSchedule);
                Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(_period).Repeat.Any();
                Expect.Call(_scheduleDayPro.Day).Return(_date1).Repeat.Any();
                Expect.Call(_contractSchedule.IsWorkday(_period.StartDate, _date1)).Return(false);
                Expect.Call(() => _scheduleDay.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
                Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay));
            }

            using (_mocks.Playback())
            {
                _target.DayScheduled += _target_DayScheduled;
                _target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService);
                _target.DayScheduled -= _target_DayScheduled;
            }
        }

        [Test]
        public void ShouldJumpOutOfContractDaysOffIfSchedulePeriodNotValid()
        {
            _effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });

            using (_mocks.Record())
            {
                Expect.Call(_schedulePeriod.IsValid).Return(false);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.IsScheduled()).Return(true).Repeat.Any();
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService);
            }        
        }

        [Test]
        public void ShouldJumpOutOfContractDaysOffIfEmploymentTypeIsHourlyStaff()
        {
            _effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });

            using (_mocks.Record())
            {
                Expect.Call(_schedulePeriod.IsValid).Return(true);
                Expect.Call(_schedulePeriod.Contract).Return(_contract);
                Expect.Call(_contract.EmploymentType).Return(EmploymentType.HourlyStaff);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.IsScheduled()).Return(true).Repeat.Any();
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService);
            }       
        }

        [Test]
        public void ShouldJumpOutOfContractDaysOffIfHasCorrectNumberOfDaysOffAndDaysOffIsNotZero()
        {
            _effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });

            using (_mocks.Record())
            {
                Expect.Call(_schedulePeriod.IsValid).Return(true);
                Expect.Call(_schedulePeriod.Contract).Return(_contract);
                Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.IsScheduled()).Return(true).Repeat.Any();
                int x;
                int y;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(1, 1);
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService);
            }       
        }

        [Test]
        public void ShouldNotAddContractDayOffWhenAllHaveBeenAdded()
        {
            _effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });

            using (_mocks.Record())
            {
                Expect.Call(_schedulePeriod.IsValid).Return(true);
                Expect.Call(_schedulePeriod.Contract).Return(_contract);
                Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.IsScheduled()).Return(true).Repeat.Any();
                int x;
                int y;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(0, 0);
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService);
            }       
        }

        [Test]
        public void ShouldNotAddContractDayOffWhenContractWorkday()
        {
            _effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;
            var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });

            using (_mocks.Record())
            {
                Expect.Call(_schedulePeriod.IsValid).Return(true);
                Expect.Call(_schedulePeriod.Contract).Return(_contract);
                Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.IsScheduled()).Return(true).Repeat.Any();
                int x;
                int y;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(1, 0);
                Expect.Call(_scheduleAvailableForDayOffSpecification.IsSatisfiedBy(_scheduleDay)).Return(true);
                Expect.Call(_schedulePeriod.ContractSchedule).Return(_contractSchedule);
                Expect.Call(_contractSchedule.IsWorkday(_period.StartDate, _date1)).Return(true);
                Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(_period).Repeat.Any();
                Expect.Call(_scheduleDayPro.Day).Return(_date1).Repeat.Any();
            }

            using (_mocks.Playback())
            {
                _target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService);
            }       
        }

        void _target_DayScheduled(object sender, SchedulingServiceBaseEventArgs e)
        {
            e.Cancel = true;
        }

        private void MocksForNotAddingContractDaysoff()
        {
            Expect.Call(_schedulePeriod.IsValid).Return(false);
        }
	}
}