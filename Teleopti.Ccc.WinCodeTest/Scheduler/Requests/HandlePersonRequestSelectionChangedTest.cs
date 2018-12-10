using System;
using System.Collections.Generic;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.Requests
{
    [TestFixture]
    public class HandlePersonRequestSelectionChangedTest
    {
        HandlePersonRequestSelectionChanged _target;
        PersonRequestViewModel _personRequestViewModel;
        IPersonRequest _source;
        IPerson _person;
        DateTimePeriod _period;
        IPersonPeriod _personPeriod;
        AccountTime _personAccount;
        IShiftTradeRequestStatusChecker _shiftTradeRequestStatusChecker;
        private IEventAggregator _eventAggregator;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _mocks=new MockRepository();
            _person = PersonFactory.CreatePerson("First", "Last");
            _personAccount = new AccountTime(new DateOnly(2008, 1, 1));
            _personAccount.BalanceIn = TimeSpan.FromHours(3);
            _eventAggregator = new EventAggregator();
            var absenceAccount = new PersonAbsenceAccount(_person, AbsenceFactory.CreateAbsence("kanin"));
            absenceAccount.Add(_personAccount);
            var accountForPersonCollection = new PersonAccountCollection(_person);
            accountForPersonCollection.Add(absenceAccount);
            
            _personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2005, 1, 1), TeamFactory.CreateSimpleTeam());
            _person.AddPersonPeriod(_personPeriod);
            _period = new DateTimePeriod(new DateTime(2000, 1, 1, 8, 45, 0, DateTimeKind.Utc),
                                         new DateTime(2001, 1, 1, 10, 45, 0, DateTimeKind.Utc));

            _source = CreateRequestObject(_person, _period);
            _shiftTradeRequestStatusChecker = new ShiftTradeRequestStatusCheckerForTestDoesNothing();

            _personRequestViewModel = new PersonRequestViewModel(_source, _shiftTradeRequestStatusChecker, accountForPersonCollection, _eventAggregator, null);
            
        }

        [Test]
        public void VerifyIsEditableIfContainsAnyEditableViewModels()
        {
            ISpecification<IPersonRequestViewModel> specification =
                _mocks.StrictMock<ISpecification<IPersonRequestViewModel>>();

            IList<PersonRequestViewModel> targetList = new List<PersonRequestViewModel>();
            targetList.Add(_personRequestViewModel);

            //Check the default specification
            _target = new HandlePersonRequestSelectionChanged(targetList, new PersonRequestAuthorizationCheckerForTest());
            Assert.IsNotNull(_target.SelectionIsEditableSpecification as PersonRequestViewModelIsEditableSpecification, "verify the default specification");
            
            //Verify it calls specification and returns true if there are any editable items
            _target = new HandlePersonRequestSelectionChanged(targetList,specification);

            using(_mocks.Record())
            {
                Expect.Call(specification.IsSatisfiedBy(_personRequestViewModel)).Return(true);
                Expect.Call(specification.IsSatisfiedBy(_personRequestViewModel)).Return(false);
            }
            using(_mocks.Playback())
            {
                Assert.IsTrue(_target.SelectionIsEditable);
                Assert.IsFalse(_target.SelectionIsEditable);
            }
        }

        private static IPersonRequest CreateRequestObject(IPerson person, DateTimePeriod period)
        {
            IAbsence absence = AbsenceFactory.CreateAbsence("absence");
            IAbsenceRequest part = new AbsenceRequest(absence, period);

            return new PersonRequest(person, part);
        }
    }
}
