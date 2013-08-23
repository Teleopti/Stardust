using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

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
        private MockRepository _mocks;
        private IScenario _scenario;
        private IAbsence _selectedItem;
        private DateTimePeriod _period;
        private IGridlockManager _gridlockManager;
        private IAddLayerViewModel<IAbsence> _dialog;
        private IPerson _person;
        private readonly DateTime _date = new DateTime(2012, 07, 16, 0, 0, 0, DateTimeKind.Utc);
        private IScheduleDay _schedulePart;
		private IScheduleDay _schedulePart2;
        private IScheduleRange _scheduleRange;
        private DateOnlyPeriod _dateOnlyPeriod;
		private IPrincipalAuthorization _principalAuthorization;
        private IList<IPersonAssignment> _personAssignmentsList = new List<IPersonAssignment>();
        private IPersonAssignment _personAssignment;
        private ReadOnlyCollection<IPersonAssignment> readOnlyCollection;
        private PersonAssignment _personAssignment2;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _viewBase = _mocks.DynamicMock<IScheduleViewBase>();
            _scenario = _mocks.DynamicMock<IScenario>();
            _dialog = _mocks.StrictMock<IAddLayerViewModel<IAbsence>>();
            _period = new DateTimePeriod(2012, 7, 16, 2012, 7, 16);
            _dateOnlyPeriod = new DateOnlyPeriod(2012, 7, 16, 2012, 7, 16);
            _selectedSchedules = new List<IScheduleDay>();
            _schedulePart = _mocks.DynamicMock<IScheduleDay>();
            _schedulerStateHolder = new SchedulerStateHolder(_scenario, new DateOnlyPeriodAsDateTimePeriod(_dateOnlyPeriod, TeleoptiPrincipal.Current.Regional.TimeZone), new List<IPerson>());
			_schedulePart2 = _mocks.DynamicMock<IScheduleDay>();
            _selectedItem = _mocks.StrictMock<IAbsence>();
            _schedulePresenterBase = _mocks.DynamicMock<ISchedulePresenterBase>();
			_principalAuthorization = _mocks.StrictMock<IPrincipalAuthorization>();
			_target = new AddAbsenceCommand(_schedulerStateHolder, _viewBase, _schedulePresenterBase, _selectedSchedules, _principalAuthorization);
            _person = PersonFactory.CreatePerson("person");
            _gridlockManager = _mocks.DynamicMock<IGridlockManager>();
            _scheduleRange = _mocks.DynamicMultiMock<IScheduleRange>(typeof(IValidateScheduleRange));
            IPerson person1 = PersonFactory.CreatePersonWithGuid("Test", "Person1");
            _personAssignment = new PersonAssignment(person1, _scenario, new DateOnly(2012, 07, 16));
            _personAssignment2 = new PersonAssignment(person1, _scenario, new DateOnly(2012, 07, 17));
			//IShiftCategory shiftCategory = ShiftCategoryFactory.ShiftCategoryWithId();
			//_personAssignment.SetMainShift(new MainShift(shiftCategory));
			//new EditableShiftMapper().SetMainShiftLayers(_personAssignment, EditableShiftFactory.CreateEditorShift(new Activity("hej"), _personAssignment.Period, shiftCategory));
            _personAssignmentsList.Add(_personAssignment);
            _personAssignmentsList.Add(_personAssignment2);
            readOnlyCollection = new ReadOnlyCollection<IPersonAssignment>(_personAssignmentsList);

        }

        [Test]
        public void DoNotCountAbsenceIfUserCancelAbsenceRequest()
        {
            using (_mocks.Record())
            {
                _selectedSchedules.Add(_schedulePart);
                var period = new DateTimePeriod(_date.AddHours(1), _date.AddHours(22));
                var scheduleDictionary = CreateExpectationForModifySchedulePart(_schedulePart, _person);

                ExpectCallsDialogOnShouldAddAbsenceWithDefaultPeriod(period);
                ExpectCallsViewBaseOnShouldAddAbsenceWithDefaultPeriod();

                Expect.Call(_schedulePresenterBase.LockManager).Return(_gridlockManager);
                Expect.Call(_schedulePart.Period).Return(DateTimeFactory.CreateDateTimePeriod(_date, 0)).Repeat.Any();
                Expect.Call(() => _schedulePart.CreateAndAddAbsence(null)).IgnoreArguments().Repeat.Once();
                Expect.Call(scheduleDictionary[_person]).Return(_scheduleRange).Repeat.Times(2);
                Expect.Call(_schedulePart.PersonAssignmentCollectionDoNotUse())
                      .Return(readOnlyCollection);
                Expect.Call(_scheduleRange.ScheduledDay(new DateOnly(2012, 07, 16))).Return(_schedulePart);
                Expect.Call(_schedulePart.TimeZone).Return(TimeZoneInfoFactory.StockholmTimeZoneInfo());
                Expect.Call(_schedulePresenterBase.ModifySchedulePart(_selectedSchedules)).Return(false);
                Expect.Call(() => ((IValidateScheduleRange)_scheduleRange).ValidateBusinessRules(null)).IgnoreArguments();
                _schedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;
            }
            using (_mocks.Playback())
            {
                _target.Execute();
            }
        }


        [Test]
        public void DoNotCountAbsenceIfMultipleAgentCancelAbsenceRequest()
        {
            using (_mocks.Record())
            {
                _selectedSchedules.Add(_schedulePart);
                var period = new DateTimePeriod(_date.AddHours(1), _date.AddHours(22));
                var scheduleDictionary = CreateExpectationForModifySchedulePart(_schedulePart, _person);

                ExpectCallsDialogOnShouldAddAbsenceWithDefaultPeriod(period);
                ExpectCallsViewBaseOnShouldAddAbsenceWithDefaultPeriod();

                Expect.Call(_schedulePresenterBase.LockManager).Return(_gridlockManager);
                Expect.Call(_schedulePart.Period).Return(DateTimeFactory.CreateDateTimePeriod(_date, 0)).Repeat.Any();
                Expect.Call(() => _schedulePart.CreateAndAddAbsence(null)).IgnoreArguments().Repeat.Once();
                Expect.Call(scheduleDictionary[_person]).Return(_scheduleRange).Repeat.Times(2);
                Expect.Call(_schedulePart.PersonAssignmentCollectionDoNotUse())
                      .Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));
                Expect.Call(_scheduleRange.ScheduledDay(new DateOnly(2012, 07, 16))).Return(_schedulePart);
                Expect.Call(_schedulePart.TimeZone).Return(TimeZoneInfoFactory.StockholmTimeZoneInfo());
                Expect.Call(_schedulePresenterBase.ModifySchedulePart(_selectedSchedules)).Return(false);
                Expect.Call(() => ((IValidateScheduleRange)_scheduleRange).ValidateBusinessRules(null)).IgnoreArguments();
                _schedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;
            }
            using (_mocks.Playback())
            {
                _target.Execute();
            }
        }
		[Test]
		public void CreateCloneScheduleForEveryDaysAffectedByAbsence()
		{
			using (_mocks.Record())
			{
				_selectedSchedules.Add(_schedulePart);
				var twoDaysPeriod = new DateTimePeriod(_date.AddHours(1), _date.AddHours(49));
				var scheduleDictionary = CreateExpectationForModifyScheduleParts(new Collection<IScheduleDay> { _schedulePart, _schedulePart2 }, _person);

				ExpectCallsDialogOnShouldAddAbsenceWithDefaultPeriod(twoDaysPeriod);
				ExpectCallsViewBaseOnShouldAddAbsenceWithDefaultPeriod();

				Expect.Call(_schedulePresenterBase.LockManager).Return(_gridlockManager);
				Expect.Call(_schedulePart.Period).Return(new DateTimePeriod(_date, _date.AddDays(1))).Repeat.Any();
				Expect.Call(_schedulePart2.Period).Return(new DateTimePeriod(_date.AddDays(1), _date.AddDays(2))).Repeat.Any();
				Expect.Call(() => _schedulePart.CreateAndAddAbsence(null)).IgnoreArguments().Repeat.Once(); // only add absence to the first day
				Expect.Call(scheduleDictionary[_person]).Return(_scheduleRange).Repeat.AtLeastOnce();
				Expect.Call(_scheduleRange.ScheduledDay(new DateOnly(2012, 07, 16))).Return(_schedulePart);
				Expect.Call(_scheduleRange.ScheduledDay(new DateOnly(2012, 07, 17))).Return(_schedulePart2);
                Expect.Call(_schedulePart.PersonAssignmentCollectionDoNotUse())
                      .Return(readOnlyCollection);
				Expect.Call(_schedulePart.TimeZone).Return(TimeZoneInfoFactory.StockholmTimeZoneInfo());
				Expect.Call(() => ((IValidateScheduleRange)_scheduleRange).ValidateBusinessRules(null)).IgnoreArguments();
				_schedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;
			}
			using (_mocks.Playback())
			{
				_target.Execute();
			}
		}

		[Test]
		public void ShouldNotAddAbsenceOnLockedDay()
		{
			var dateOnly = new DateOnly(2012, 07, 16);
			var gridLockManager = new GridlockManager();
			gridLockManager.AddLock(_person, dateOnly, LockType.Normal, _period);

			_selectedSchedules.Add(_schedulePart);
			var period = new DateTimePeriod(_date.AddHours(1), _date.AddHours(22));
			var scheduleDictionary = CreateExpectationForModifySchedulePart(_schedulePart, _person);

			using (_mocks.Record())
			{
				ExpectCallsDialogOnShouldAddAbsenceWithDefaultPeriod(period);
				ExpectCallsViewBaseOnShouldAddAbsenceWithDefaultPeriod();

				Expect.Call(_schedulePresenterBase.LockManager).Return(gridLockManager);
				Expect.Call(_schedulePart.Period).Return(DateTimeFactory.CreateDateTimePeriod(_date, 0)).Repeat.Any();
				Expect.Call(scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(dateOnly)).Return(_schedulePart);
                Expect.Call(_schedulePart.PersonAssignmentCollectionDoNotUse())
                      .Return(readOnlyCollection);
				Expect.Call(_schedulePart.TimeZone).Return(TimeZoneInfoFactory.StockholmTimeZoneInfo());
				_schedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;
			}
			using (_mocks.Playback())
			{
				_target.Execute();
			}
		}

		[Test]
		public void ShouldNotAddAbsenceNoPermissionOnWriteProtectedDay()
		{
			var dateOnly = new DateOnly(2012, 07, 16);
			var gridLockManager = new GridlockManager();
			gridLockManager.AddLock(_person, dateOnly, LockType.WriteProtected, _period);

			_selectedSchedules.Add(_schedulePart);
			var period = new DateTimePeriod(_date.AddHours(1), _date.AddHours(22));
			var scheduleDictionary = CreateExpectationForModifySchedulePart(_schedulePart, _person);

			using (_mocks.Record())
			{
				ExpectCallsDialogOnShouldAddAbsenceWithDefaultPeriod(period);
				ExpectCallsViewBaseOnShouldAddAbsenceWithDefaultPeriod();

				Expect.Call(_principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule)).Return(false);
				Expect.Call(_schedulePresenterBase.LockManager).Return(gridLockManager);
				Expect.Call(_schedulePart.Period).Return(DateTimeFactory.CreateDateTimePeriod(_date, 0)).Repeat.Any();
				Expect.Call(scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(dateOnly)).Return(_schedulePart);
				Expect.Call(_schedulePart.TimeZone).Return(TimeZoneInfoFactory.StockholmTimeZoneInfo());
                Expect.Call(_schedulePart.PersonAssignmentCollectionDoNotUse())
                      .Return(readOnlyCollection);
				_schedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;
			}
			using (_mocks.Playback())
			{
				_target.Execute();
			}
		}

		[Test]
		public void ShouldAddAbsencePermissionOnWriteProtectedDay()
		{
			var dateOnly = new DateOnly(2012, 07, 16);
			var gridLockManager = new GridlockManager();
			gridLockManager.AddLock(_person, dateOnly, LockType.WriteProtected, _period);

			_selectedSchedules.Add(_schedulePart);
			var period = new DateTimePeriod(_date.AddHours(1), _date.AddHours(22));
			var scheduleDictionary = CreateExpectationForModifySchedulePart(_schedulePart, _person);

			using (_mocks.Record())
			{
				ExpectCallsDialogOnShouldAddAbsenceWithDefaultPeriod(period);
				ExpectCallsViewBaseOnShouldAddAbsenceWithDefaultPeriod();

				Expect.Call(_schedulePresenterBase.LockManager).Return(gridLockManager);
				Expect.Call(_principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule)).Return(true);
				Expect.Call(_schedulePart.Period).Return(new DateTimePeriod(_date, _date.AddDays(1))).Repeat.Any();
				Expect.Call(_schedulePart2.Period).Return(new DateTimePeriod(_date.AddDays(1), _date.AddDays(2))).Repeat.Any();
				Expect.Call(() => _schedulePart.CreateAndAddAbsence(null)).IgnoreArguments().Repeat.Once(); // only add absence to the first day
				Expect.Call(scheduleDictionary[_person]).Return(_scheduleRange).Repeat.AtLeastOnce();
				Expect.Call(_scheduleRange.ScheduledDay(new DateOnly(2012, 07, 16))).Return(_schedulePart);
				Expect.Call(_schedulePart.TimeZone).Return(TimeZoneInfoFactory.StockholmTimeZoneInfo());
				Expect.Call(() => ((IValidateScheduleRange)_scheduleRange).ValidateBusinessRules(null)).IgnoreArguments();
                Expect.Call(_schedulePart.PersonAssignmentCollectionDoNotUse())
                      .Return(readOnlyCollection);
				_schedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;
			}
			using (_mocks.Playback())
			{
				_target.Execute();
			}
		}

        private IScheduleDictionary CreateExpectationForModifySchedulePart(IScheduleDay schedulePart, IPerson person)
        {
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            Expect.Call(schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(_date), (TimeZoneInfo.Utc))).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.Person).Return(person).Repeat.AtLeastOnce();
            return scheduleDictionary;
        }

		private IScheduleDictionary CreateExpectationForModifyScheduleParts(IEnumerable<IScheduleDay> scheduleParts, IPerson person)
		{
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var startDate = _date;
			foreach (IScheduleDay schedulePart in scheduleParts)
			{
				Expect.Call(schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(startDate), (TimeZoneInfo.Utc))).Repeat.AtLeastOnce();
				Expect.Call(schedulePart.Person).Return(person).Repeat.AtLeastOnce();
				startDate = startDate.AddDays(1);

			}

			return scheduleDictionary;
		}


        private void ExpectCallsViewBaseOnShouldAddAbsenceWithDefaultPeriod()
        {
            Expect.Call(_viewBase.SelectedSchedules()).Return(_selectedSchedules);
			Expect.Call(_viewBase.CreateAddAbsenceViewModel(null, null, TimeZoneInfo.Local)).IgnoreArguments().Return(_dialog);
        }

        private void ExpectCallsDialogOnShouldAddAbsenceWithDefaultPeriod(DateTimePeriod period)
        {
            Expect.Call(_dialog.Result).Return(true);
            Expect.Call(_dialog.SelectedItem).Return(_selectedItem);
            Expect.Call(_dialog.SelectedPeriod).Return(period);
        }

    }
}
