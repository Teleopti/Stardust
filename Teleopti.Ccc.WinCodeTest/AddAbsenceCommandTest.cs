using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest
{
	[TestFixture]
	public class AddAbsenceCommandTest
	{
		private AddAbsenceCommand _target;
		private ISchedulePresenterBase _schedulePresenterBase;
		private IScheduleViewBase _viewBase;
		private ISchedulerStateHolder _schedulerStateHolder;
		private IList<IScheduleDay> _selectedSchedules;
		private IScenario _scenario;
		private IAbsence _selectedItem;
		private IGridlockManager _gridlockManager;
		private IAddLayerViewModel<IAbsence> _dialog;
		private IPerson _person;
		private readonly DateTime _date = new DateTime(2012, 07, 16, 0, 0, 0, DateTimeKind.Utc);
		private IScheduleDay _schedulePart;
		private IScheduleRange _scheduleRange;
		private DateOnlyPeriod _dateOnlyPeriod;
		private IAuthorization _authorization;
		private IScheduleDictionary _scheduleDictionary;

		[SetUp]
		public void Setup()
		{
			_viewBase = MockRepository.GenerateMock<IScheduleViewBase>();
			_scenario = MockRepository.GenerateMock<IScenario>();
			_dialog = MockRepository.GenerateMock<IAddLayerViewModel<IAbsence>>();
			_dateOnlyPeriod = new DateOnlyPeriod(2012, 7, 16, 2012, 7, 16);
			_schedulePart = MockRepository.GenerateMock<IScheduleDay>();
			_scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			_schedulerStateHolder = new SchedulerStateHolder(_scenario, new DateOnlyPeriodAsDateTimePeriod(_dateOnlyPeriod, TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone), new List<IPerson>(), MockRepository.GenerateMock<IDisableDeletedFilter>(), new SchedulingResultStateHolder(), new TimeZoneGuard());
			_selectedItem = MockRepository.GenerateMock<IAbsence>();
			_schedulePresenterBase = MockRepository.GenerateMock<ISchedulePresenterBase>();
			_authorization = MockRepository.GenerateMock<IAuthorization>();
			
			_target = new AddAbsenceCommand(_schedulerStateHolder, _viewBase, _schedulePresenterBase, _selectedSchedules,new ThisAuthorization(_authorization));
			_person = PersonFactory.CreatePerson("person");
			_gridlockManager = new GridlockManager();
			_scheduleRange = (IScheduleRange) MockRepository.GenerateMock(typeof (IScheduleRange), new[] {typeof (IValidateScheduleRange)});
			
			_schedulePart.Stub(x => x.Person).Return(_person);
			_schedulePart.Stub(x => x.Period).Return(new DateTimePeriod(_date, _date.AddDays(1)));
			_schedulePart.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_dateOnlyPeriod.StartDate, TimeZoneInfoFactory.StockholmTimeZoneInfo()));
			_schedulePart.Stub(x => x.TimeZone).Return(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			_scheduleRange.Stub(x => x.ScheduledDay(new DateOnly(2012, 07, 16))).Return(_schedulePart);
			_schedulerStateHolder.SchedulingResultState.Schedules = _scheduleDictionary;
			_scheduleDictionary.Stub(x => x[_person]).Return(_scheduleRange);

			_selectedSchedules = new List<IScheduleDay>{_schedulePart};

			_viewBase.Stub(x => x.SelectedSchedules()).Return(_selectedSchedules);
			_viewBase.Stub(x => x.CreateAddAbsenceViewModel(null, null, TimeZoneInfo.Local)).IgnoreArguments().Return(_dialog);

			_dialog.Stub(x => x.Result).Return(true);
			_dialog.Stub(x => x.SelectedItem).Return(_selectedItem);
		}

		[Test]
		public void DoNotCountAbsenceIfUserCancelAbsenceRequest()
		{
			var period = new DateTimePeriod(_date.AddHours(1), _date.AddHours(22));

			_dialog.Stub(x => x.SelectedPeriod).Return(period);
			_schedulePresenterBase.Stub(x => x.LockManager).Return(_gridlockManager);
			_scheduleRange.Stub(x => x.ScheduledDay(new DateOnly(2012, 07, 16))).Return(_schedulePart);
			
			_target.Execute();

			_schedulePart.AssertWasCalled(x => x.CreateAndAddAbsence(null), o => o.IgnoreArguments().Repeat.Once());
			_viewBase.AssertWasNotCalled(x => x.RefreshRangeForAgentPeriod(null,new DateTimePeriod()), o=> o.IgnoreArguments());
		}


		[Test]
		public void DoNotCountAbsenceIfMultipleAgentCancelAbsenceRequest()
		{
			var period = new DateTimePeriod(_date.AddHours(1), _date.AddHours(22));

			_dialog.Stub(x => x.SelectedPeriod).Return(period);
			_schedulePresenterBase.Stub(x => x.LockManager).Return(_gridlockManager);
			_scheduleRange.Stub(x => x.ScheduledDay(new DateOnly(2012, 07, 16))).Return(_schedulePart);
			_schedulePresenterBase.Stub(x =>x.ModifySchedulePart(_selectedSchedules)).Return(false);

			_target.Execute();

			_schedulePart.AssertWasCalled(x => x.CreateAndAddAbsence(null), o => o.IgnoreArguments().Repeat.Once());
			_viewBase.AssertWasNotCalled(x => x.RefreshRangeForAgentPeriod(null, new DateTimePeriod()), o => o.IgnoreArguments());
		}

		[Test]
		public void CreateCloneScheduleForEveryDaysAffectedByAbsence()
		{
			var twoDaysPeriod = new DateTimePeriod(_date.AddHours(1), _date.AddHours(49));
			var schedulePart2 = MockRepository.GenerateMock<IScheduleDay>();

			_dialog.Stub(x => x.SelectedPeriod).Return(twoDaysPeriod);
			_schedulePresenterBase.Stub(x => x.LockManager).Return(_gridlockManager);
			_scheduleRange.Stub(x => x.ScheduledDay(new DateOnly(2012, 07, 17))).Return(schedulePart2);
			_schedulePresenterBase.Stub(x => x.ModifySchedulePart(_selectedSchedules)).Return(false);
			schedulePart2.Stub(x => x.Person).Return(_person);
			schedulePart2.Stub(x => x.Period).Return(new DateTimePeriod(_date.AddDays(1), _date.AddDays(2)));
			schedulePart2.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_dateOnlyPeriod.StartDate.AddDays(1), TimeZoneInfoFactory.StockholmTimeZoneInfo()));

			_target.Execute();
		}

		[Test]
		public void ShouldNotAddAbsenceOnLockedDay()
		{
			var dateOnly = new DateOnly(2012, 07, 16);
			var gridLockManager = new GridlockManager();
			gridLockManager.AddLock(_person, dateOnly, LockType.Normal);

			var period = new DateTimePeriod(_date.AddHours(1), _date.AddHours(22));

			_dialog.Stub(x => x.SelectedPeriod).Return(period);
			_schedulePresenterBase.Stub(x => x.LockManager).Return(gridLockManager);
			_scheduleRange.Stub(x => x.ScheduledDay(new DateOnly(2012, 07, 16))).Return(_schedulePart);

			_target.Execute();

			_schedulePart.AssertWasNotCalled(x => x.CreateAndAddAbsence(null), o => o.IgnoreArguments().Repeat.Once());
		}

		[Test]
		public void ShouldNotAddAbsenceNoPermissionOnWriteProtectedDay()
		{
			var dateOnly = new DateOnly(2012, 07, 16);
			var gridLockManager = new GridlockManager();
			gridLockManager.AddLock(_person, dateOnly, LockType.WriteProtected);

			var period = new DateTimePeriod(_date.AddHours(1), _date.AddHours(22));

			_dialog.Stub(x => x.SelectedPeriod).Return(period);
			_authorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule)).Return(false);
			_schedulePresenterBase.Stub(x => x.LockManager).Return(gridLockManager);
			_scheduleRange.Stub(x => x.ScheduledDay(new DateOnly(2012, 07, 16))).Return(_schedulePart);

			_target.Execute();

			_schedulePart.AssertWasNotCalled(x => x.CreateAndAddAbsence(null), o => o.IgnoreArguments().Repeat.Once());
		}

		[Test]
		public void ShouldAddAbsencePermissionOnWriteProtectedDay()
		{
			var dateOnly = new DateOnly(2012, 07, 16);
			var gridLockManager = new GridlockManager();
			gridLockManager.AddLock(_person, dateOnly, LockType.WriteProtected);

			var period = new DateTimePeriod(_date.AddHours(1), _date.AddHours(22));
			var schedulePart2 = MockRepository.GenerateMock<IScheduleDay>();

			_dialog.Stub(x => x.SelectedPeriod).Return(period);
			_authorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule)).Return(true);
			_schedulePresenterBase.Stub(x => x.LockManager).Return(gridLockManager);
			_scheduleRange.Stub(x => x.ScheduledDay(new DateOnly(2012, 07, 16))).Return(_schedulePart); 
			_scheduleRange.Stub(x => x.ScheduledDay(new DateOnly(2012, 07, 17))).Return(schedulePart2);
			_schedulePresenterBase.Stub(x => x.ModifySchedulePart(_selectedSchedules)).Return(false);
			schedulePart2.Stub(x => x.Person).Return(_person);
			schedulePart2.Stub(x => x.Period).Return(new DateTimePeriod(_date.AddDays(1), _date.AddDays(2)));
			schedulePart2.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_dateOnlyPeriod.StartDate.AddDays(1), TimeZoneInfoFactory.StockholmTimeZoneInfo()));
			
			_target.Execute();

			_schedulePart.AssertWasCalled(x => x.CreateAndAddAbsence(null),
			                                 o =>
			                                 o.IgnoreArguments()
			                                  .Repeat.Once()
			                                  .Constraints(
				                                  Rhino.Mocks.Constraints.Is.Matching<IAbsenceLayer>(p => p.Period.StartDateTime == period.StartDateTime)));
		}


		[Test]
		public void ShouldUseDefaultPeriodInDialogIfIsSingelAgentRequest()
		{
			var viewBase = MockRepository.GenerateMock<IScheduleViewBase>();
			var start = new DateTime(2012, 7, 16, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2012, 7, 16, 12, 0, 0, DateTimeKind.Utc);
			var defaultPeriod = new DateTimePeriod(start, end);
			var dialog = MockRepository.GenerateMock<IAddLayerViewModel<IAbsence>>();
			_target = new AddAbsenceCommand(_schedulerStateHolder, viewBase, _schedulePresenterBase, _selectedSchedules, new ThisAuthorization(_authorization));

			viewBase.Stub(x => x.SelectedSchedules()).Return(_selectedSchedules);
			viewBase.Stub(x => x.CreateAddAbsenceViewModel(null, null, TimeZoneInfoFactory.StockholmTimeZoneInfo()))
						.Constraints(Rhino.Mocks.Constraints.Is.Anything(),
						new Rhino.Mocks.Constraints.PredicateConstraint<ISetupDateTimePeriod>(period => period.Period == defaultPeriod),
						Rhino.Mocks.Constraints.Is.Anything()).Return(dialog);

			dialog.Stub(x => x.Result).Return(false);

			_target.DefaultPeriod = defaultPeriod;
			_target.Execute();
		}
	}
}
