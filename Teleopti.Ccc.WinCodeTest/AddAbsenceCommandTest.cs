using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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
        private IScheduleRange _scheduleRange;
        private DateOnlyPeriod _dateOnlyPeriod;

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
            _selectedItem = _mocks.StrictMock<IAbsence>();
            _schedulePresenterBase = _mocks.DynamicMock<ISchedulePresenterBase>();
            _target = new AddAbsenceCommand(_schedulerStateHolder, _viewBase, _schedulePresenterBase, _selectedSchedules);
            _person = PersonFactory.CreatePerson("person");
            _gridlockManager = _mocks.DynamicMock<IGridlockManager>();
            _scheduleRange = _mocks.DynamicMultiMock<IScheduleRange>(typeof(IValidateScheduleRange));
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
                Expect.Call(_scheduleRange.ScheduledDay(new DateOnly(2012, 07, 16))).Return(_schedulePart);
                Expect.Call(_schedulePart.TimeZone).Return(CccTimeZoneInfoFactory.StockholmTimeZoneInfo());
                Expect.Call(_schedulePresenterBase.ModifySchedulePart(_selectedSchedules)).Return(false);
                Expect.Call(() => ((IValidateScheduleRange)_scheduleRange).ValidateBusinessRules(null)).IgnoreArguments();
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
            Expect.Call(schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(_date), new CccTimeZoneInfo(TimeZoneInfo.Utc))).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.Person).Return(person).Repeat.AtLeastOnce();
            return scheduleDictionary;
        }


        private void ExpectCallsViewBaseOnShouldAddAbsenceWithDefaultPeriod()
        {
            Expect.Call(_viewBase.SelectedSchedules()).Return(_selectedSchedules);
            Expect.Call(_viewBase.CreateAddAbsenceViewModel(null, null)).IgnoreArguments().Return(_dialog);
        }

        private void ExpectCallsDialogOnShouldAddAbsenceWithDefaultPeriod(DateTimePeriod period)
        {
            Expect.Call(_dialog.Result).Return(true);
            Expect.Call(_dialog.SelectedItem).Return(_selectedItem);
            Expect.Call(_dialog.SelectedPeriod).Return(period);
        }

    }
}
