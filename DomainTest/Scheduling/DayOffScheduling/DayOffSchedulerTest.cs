using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.DayOffScheduling
{
	[TestFixture]
	public class DayOffSchedulerTest
	{
		private MockRepository _mocks;

		private IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private ISchedulingOptions _schedulingOptions;
		private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private IEffectiveRestriction _effectiveRestriction;
		private IDayOffScheduler _target;
		private DateOnly _date1 = new DateOnly(2009, 2, 2);
		private IScheduleDayAvailableForDayOffSpecification _scheduleAvailableForDayOffSpecification;
		private IVirtualSchedulePeriod _schedulePeriod;
		private DateOnlyPeriod _period;
		private IScheduleDayPro _scheduleDayPro;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IScheduleDay _scheduleDay;
		private IScheduleDayPro _scheduleDayPro2;
		private IHasContractDayOffDefinition _hasContractDayOffDefinition;
		private IPersonAssignment _personAssignment;
		private IPrincipalAuthorization _principalAuthorization;
		private IPerson _person;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
		private IScheduleTagSetter _scheduleTagSetter;


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_period = new DateOnlyPeriod(_date1, _date1.AddDays(1));
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
			_hasContractDayOffDefinition = _mocks.StrictMock<IHasContractDayOffDefinition>();
			_target = new DayOffScheduler(_dayOffsInPeriodCalculator, _effectiveRestrictionCreator,
										  ()=>_schedulePartModifyAndRollbackService, _scheduleAvailableForDayOffSpecification,
										  _hasContractDayOffDefinition);


			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_schedulingOptions.DayOffTemplate = new DayOffTemplate(new Description("hej"));
			_scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
			_personAssignment = _mocks.StrictMock<IPersonAssignment>();
			_principalAuthorization = _mocks.StrictMock<IPrincipalAuthorization>();
			_person = _mocks.StrictMock<IPerson>();
			_dateOnlyAsDateTimePeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
			_scheduleTagSetter = new ScheduleTagSetter(new NullScheduleTag());
		}

		[Test]
		public void ShouldAddPreferenceDayOff()
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
				Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay, _scheduleTagSetter));

				mocksForNotAddingContractDaysoff();
			}

			using (_mocks.Playback())
			{
				_target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService, _schedulingOptions, _scheduleTagSetter);
			}
		}

		[Test]
		public void ShouldRollbackIfOnOutsideScheduleExceptionWhenAddPreferenceDayOff()
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
				Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay, _scheduleTagSetter)).Throw(new DayOffOutsideScheduleException());
				Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback());

				mocksForNotAddingContractDaysoff();
			}

			using (_mocks.Playback())
			{
				_target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService, _schedulingOptions, _scheduleTagSetter);
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

				mocksForNotAddingContractDaysoff();
			}

			using (_mocks.Playback())
			{
				_target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService, _schedulingOptions, _scheduleTagSetter);
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
				Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay, _scheduleTagSetter));

				mocksForNotAddingContractDaysoff();
			}

			using (_mocks.Playback())
			{
				_target.DayScheduled += _target_DayScheduled;
				_target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService, _schedulingOptions, _scheduleTagSetter);
				_target.DayScheduled -= _target_DayScheduled;
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

				mocksForNotAddingContractDaysoff();
			}

			using (_mocks.Playback())
			{
				_target.DayOffScheduling(matrixProList, matrixProList, null, _schedulingOptions, _scheduleTagSetter);
			}
		}

		[Test]
		public void ShouldAddContractDayOffs()
		{
			_effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;
			var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
			var dayOffOnPeriod = _mocks.StrictMock<IDayOffOnPeriod>();
			var dayOffPeriods = new List<IDayOffOnPeriod> { dayOffOnPeriod };

			using (_mocks.Record())
			{
				Expect.Call(_schedulePeriod.IsValid).Return(true);
				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.IsScheduled()).Return(true).Repeat.Any();
				
				int x;
				IList<IScheduleDay> y = new List<IScheduleDay>();
				Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(1, y).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(_period).Repeat.Any();
				Expect.Call(_scheduleDayPro.Day).Return(_date1).Repeat.Any();
				Expect.Call(() => _scheduleDay.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
				Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay));
				Expect.Call(_dayOffsInPeriodCalculator.WeekPeriodsSortedOnDayOff(_scheduleMatrixPro)).Return(dayOffPeriods).Repeat.AtLeastOnce();
				Expect.Call(dayOffOnPeriod.FindBestSpotForDayOff(_hasContractDayOffDefinition, _scheduleAvailableForDayOffSpecification, _effectiveRestrictionCreator, _schedulingOptions)).Return(_scheduleDay);
				Expect.Call(dayOffOnPeriod.FindBestSpotForDayOff(_hasContractDayOffDefinition, _scheduleAvailableForDayOffSpecification, _effectiveRestrictionCreator, _schedulingOptions)).Return(null);
				Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_personAssignment.FunctionPath).Return("functionPath");
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_date1);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_principalAuthorization.IsPermitted("functionPath", _date1, _person)).Return(true);
			}

			using (_mocks.Playback())
			{
				using (new CustomAuthorizationContext(_principalAuthorization))
				{
					_target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService, _schedulingOptions, _scheduleTagSetter);
				}
			}	
		}

		[Test]
		public void ShouldRollbackOnOutsideScheduleExceptionOnAddContractsDayOff()
		{
			_effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;
			var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro });
			var dayOffOnPeriod = _mocks.StrictMock<IDayOffOnPeriod>();
			var dayOffPeriods = new List<IDayOffOnPeriod>{dayOffOnPeriod};

			using (_mocks.Record())
			{
				Expect.Call(_schedulePeriod.IsValid).Return(true);
				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.IsScheduled()).Return(true).Repeat.Any();
			
				int x;
				IList<IScheduleDay> y = new List<IScheduleDay>();
				Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(1, y).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(_period).Repeat.Any();
				Expect.Call(_scheduleDayPro.Day).Return(_date1).Repeat.Any();
				Expect.Call(() => _scheduleDay.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
				Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay)).Throw(new DayOffOutsideScheduleException());
				Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback());
				Expect.Call(_dayOffsInPeriodCalculator.WeekPeriodsSortedOnDayOff(_scheduleMatrixPro)).Return(dayOffPeriods);
				Expect.Call(dayOffOnPeriod.FindBestSpotForDayOff(_hasContractDayOffDefinition, _scheduleAvailableForDayOffSpecification, _effectiveRestrictionCreator, _schedulingOptions)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_personAssignment.FunctionPath).Return("functionPath");
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_date1);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_principalAuthorization.IsPermitted("functionPath", _date1, _person)).Return(true);
			
			}

			using (_mocks.Playback())
			{
				using (new CustomAuthorizationContext(_principalAuthorization))
				{
					_target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService, _schedulingOptions, _scheduleTagSetter);
				}
			}
		}

		[Test]
		public void ShouldJumpOutIfCanceledInAddContractDaysOff()
		{
			_effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;
			var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro, _scheduleDayPro2 });
			var dayOffOnPeriod = _mocks.StrictMock<IDayOffOnPeriod>();
			var dayOffPeriods = new List<IDayOffOnPeriod> { dayOffOnPeriod };

			using (_mocks.Record())
			{
				Expect.Call(_schedulePeriod.IsValid).Return(true);
				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.IsScheduled()).Return(true).Repeat.Any();
				
				int x;
				IList<IScheduleDay> y = new List<IScheduleDay>();
				Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(1, y);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(_period).Repeat.Any();
				Expect.Call(_scheduleDayPro.Day).Return(_date1).Repeat.Any();
				Expect.Call(() => _scheduleDay.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
				Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay));
				Expect.Call(_dayOffsInPeriodCalculator.WeekPeriodsSortedOnDayOff(_scheduleMatrixPro)).Return(dayOffPeriods);
				Expect.Call(dayOffOnPeriod.FindBestSpotForDayOff(_hasContractDayOffDefinition, _scheduleAvailableForDayOffSpecification, _effectiveRestrictionCreator, _schedulingOptions)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_personAssignment.FunctionPath).Return("functionPath");
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_date1);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_principalAuthorization.IsPermitted("functionPath", _date1, _person)).Return(true);
			}

			using (_mocks.Playback())
			{
				using (new CustomAuthorizationContext(_principalAuthorization))
				{
					_target.DayScheduled += _target_DayScheduled;
					_target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService, _schedulingOptions, _scheduleTagSetter);
					_target.DayScheduled -= _target_DayScheduled;
				}
			}
		}

		[Test]
		public void ShouldSkipModifyWhenNoPermissionToAddDayOff()
		{
			_effectiveRestriction.DayOffTemplate = _schedulingOptions.DayOffTemplate;
			var matrixProList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var scheduleDayProList = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro, _scheduleDayPro2 });
			var dayOffOnPeriod = _mocks.StrictMock<IDayOffOnPeriod>();
			var dayOffPeriods = new List<IDayOffOnPeriod> { dayOffOnPeriod };

			using (_mocks.Record())
			{
				Expect.Call(_schedulePeriod.IsValid).Return(true);
				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.IsScheduled()).Return(true).Repeat.Any();

				int x;
				IList<IScheduleDay> y = new List<IScheduleDay>();
				Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(1, y).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(_period).Repeat.Any();
				Expect.Call(_scheduleDayPro.Day).Return(_date1).Repeat.Any();
				Expect.Call(() => _scheduleDay.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate));
				Expect.Call(_dayOffsInPeriodCalculator.WeekPeriodsSortedOnDayOff(_scheduleMatrixPro)).Return(dayOffPeriods);
				Expect.Call(dayOffOnPeriod.FindBestSpotForDayOff(_hasContractDayOffDefinition, _scheduleAvailableForDayOffSpecification, _effectiveRestrictionCreator, _schedulingOptions)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_personAssignment.FunctionPath).Return("functionPath");
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_date1);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_principalAuthorization.IsPermitted("functionPath", _date1, _person)).Return(false);
			}

			using (_mocks.Playback())
			{
				using (new CustomAuthorizationContext(_principalAuthorization))
				{
					_target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService, _schedulingOptions, _scheduleTagSetter);
				}
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
				_target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService, _schedulingOptions, _scheduleTagSetter);
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
				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.IsScheduled()).Return(true).Repeat.Any();
				int x;
				IList<IScheduleDay> y = new List<IScheduleDay>{_scheduleDay};
				Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(1, y);
			}

			using (_mocks.Playback())
			{
				_target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService, _schedulingOptions, _scheduleTagSetter);
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
				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProList).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.IsScheduled()).Return(true).Repeat.Any();
				int x;
				IList<IScheduleDay> y = new List<IScheduleDay>();
				Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(0, y);
			}

			using (_mocks.Playback())
			{
				_target.DayOffScheduling(matrixProList, matrixProList, _schedulePartModifyAndRollbackService, _schedulingOptions, _scheduleTagSetter);
			}
		}

		void _target_DayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			e.Cancel = true;
		}

		private void mocksForNotAddingContractDaysoff()
		{
			Expect.Call(_schedulePeriod.IsValid).Return(false);
		}
	}
}