using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
    public class PersonAccountBalanceCalculatorTest
    {
        private IPersonAccountBalanceCalculator _target;
        private MockRepository _mocks;
        private IAccount _mockedAccount;
        private IScheduleDay _scheduleDay;
        private IAbsence _absence;
        private ITracker _tracker;
        private IPersonAbsenceAccount _personAbsenceAccount;
        private IList<IAccount> _listOfAccounts;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _mockedAccount = _mocks.StrictMock<IAccount>();
            _scheduleDay = _mocks.StrictMock<IScheduleDay>();
            _absence = _mocks.StrictMock<IAbsence>();
            _tracker = _mocks.StrictMock<ITracker>();
            _personAbsenceAccount = _mocks.StrictMock<IPersonAbsenceAccount>();
            _listOfAccounts = new List<IAccount> {_mockedAccount};
            _target = new PersonAccountBalanceCalculator(_listOfAccounts);
        }

        [Test]
        public void VerifyCalculate()
        {
            IScheduleRange scheduleRange = _mocks.StrictMock<IScheduleRange>();
            DateOnlyPeriod period = new DateOnlyPeriod(2010, 1, 1, 2010, 1, 1);
            IPerson person = PersonFactory.CreatePerson("Kalle");
            DateTimePeriod dtPeriod = period.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());

            using (_mocks.Record())
            {
                Expect.Call(scheduleRange.Period).Return(dtPeriod).Repeat.AtLeastOnce();
                Expect.Call(scheduleRange.Person).Return(person).Repeat.AtLeastOnce();
                Expect.Call(scheduleRange.ScheduledDayCollection(period)).Return(new List<IScheduleDay> { _scheduleDay }).
                    Repeat.AtLeastOnce();
                Expect.Call(_mockedAccount.Period()).Return(period).Repeat.AtLeastOnce();
                Expect.Call(_mockedAccount.Owner).Return(_personAbsenceAccount).Repeat.AtLeastOnce();
                Expect.Call(_personAbsenceAccount.Absence).Return(_absence).Repeat.AtLeastOnce();
                Expect.Call(_absence.Tracker).Return(_tracker).Repeat.AtLeastOnce();
                Expect.Call(() => _tracker.Track(_mockedAccount, _absence, new List<IScheduleDay> { _scheduleDay })).Repeat.
                    AtLeastOnce();
                Expect.Call(_mockedAccount.IsExceeded).Return(false).Repeat.Twice();
            }
            using(_mocks.Playback())
            {
                Assert.IsTrue(_target.CheckBalance(scheduleRange, period));
            }
        }

    }
}
