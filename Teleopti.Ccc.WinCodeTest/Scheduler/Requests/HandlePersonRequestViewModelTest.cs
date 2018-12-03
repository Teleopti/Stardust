using System;
using System.Collections.Generic;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.Requests
{
    [TestFixture]
    public class HandlePersonRequestViewModelTest
    {
        private IPersonRequest _source;
        private IPerson _person;
        private DateTimePeriod _period;
        private IPersonPeriod _personPeriod;
        private AccountTime _personAccount;
        private IShiftTradeRequestStatusChecker _shiftTradeRequestStatusChecker;
        private HandlePersonRequestViewModel _target;
        private DateTimePeriod _schedulePeriod;
        private Dictionary<IPerson, IPersonAccountCollection> _allAccounts;
        private IEventAggregator _eventAggregator;
        private TimeZoneInfo _timeZoneInfo;

        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson("First", "Last");
            _eventAggregator = new EventAggregator();
            _personAccount = new AccountTime(new DateOnly(2008, 1, 1)) {BalanceIn = TimeSpan.FromHours(3)};
            var acc = new PersonAccountCollection(_person);
            var absenceAccount = new PersonAbsenceAccount(_person, AbsenceFactory.CreateAbsence("Times"));
            absenceAccount.Add(_personAccount);
            acc.Add(absenceAccount);

            _personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2005, 1, 1), TeamFactory.CreateSimpleTeam());
            _person.AddPersonPeriod(_personPeriod);
            _period = new DateTimePeriod(new DateTime(2000, 1, 1, 8, 45, 0, DateTimeKind.Utc),
                                         new DateTime(2001, 1, 1, 10, 45, 0, DateTimeKind.Utc));
            _schedulePeriod = new DateTimePeriod(1800, 1, 1, 2999, 1, 1);
            _source = CreateRequestObject(_person, _period);
            _shiftTradeRequestStatusChecker = new ShiftTradeRequestStatusCheckerForTestDoesNothing();

            _allAccounts = new Dictionary<IPerson, IPersonAccountCollection> { { _person, acc } };
            _timeZoneInfo = TimeZoneInfo.Utc;
            _target = new HandlePersonRequestViewModel(_schedulePeriod, new List<IPerson> { _person }, new UndoRedoContainer(), _allAccounts,
                _eventAggregator, new PersonRequestAuthorizationCheckerForTest(), _timeZoneInfo);
        }

        [Test]
        public void VerifySelectedModels()
        {
			_target.CreatePersonRequestViewModels(new List<IPersonRequest> {_source,_source,_source},_shiftTradeRequestStatusChecker, new PersonRequestAuthorizationCheckerForTest());

	        _target.PersonRequestViewModels.MoveCurrentToPosition(0);
	        ((PersonRequestViewModel) _target.PersonRequestViewModels.CurrentItem).IsSelected = true;
	        _target.PersonRequestViewModels.MoveCurrentToPosition(1);
	        ((PersonRequestViewModel) _target.PersonRequestViewModels.CurrentItem).IsSelected = true;

	        _target.SelectedModels.Count.Should().Be.EqualTo(2);

			_target.PersonRequestViewModels.MoveCurrentToPosition(2);
			((PersonRequestViewModel)_target.PersonRequestViewModels.CurrentItem).IsSelected = true;

			_target.SelectedModels.Count.Should().Be.EqualTo(3);
		}

        [Test]
        public void VerifySelectedModel()
        {
			_target.CreatePersonRequestViewModels(new List<IPersonRequest> { _source }, _shiftTradeRequestStatusChecker, new PersonRequestAuthorizationCheckerForTest());

			Assert.IsNull(_target.SelectedModel);
            _target.PersonRequestViewModels.MoveCurrentToFirst();
            Assert.IsNotNull(_target.SelectedModel);
        }
		
        [Test]
        public void VerifyThatTheModelsAreOrderedByLastUpdatedByDefault()
        {

            DateTime now = DateTime.UtcNow;

            IPersonRequest request1 = CreatepersonRequestLastUpdated(now.Subtract(TimeSpan.FromDays(1)));
            IPersonRequest request2 = CreatepersonRequestLastUpdated(now.Subtract(TimeSpan.FromDays(2)));
            IPersonRequest request3 = CreatepersonRequestLastUpdated(now.Subtract(TimeSpan.FromDays(3)));
            IPersonRequest request4 = CreatepersonRequestLastUpdated(now.Subtract(TimeSpan.FromDays(4)));


            IList<IPersonRequest> personRequests = new List<IPersonRequest> { request1, request2, request4, request3 };
            _target.CreatePersonRequestViewModels(personRequests, _shiftTradeRequestStatusChecker, new PersonRequestAuthorizationCheckerForTest());


            IList<IPersonRequest> sortedList = new List<IPersonRequest> { request1, request2, request3, request4 };


            for (int i = 0; i < _target.PersonRequestViewModels.Count; i++)
            {
                _target.PersonRequestViewModels.MoveCurrentToPosition(i);
                Assert.AreEqual(sortedList[i], _target.SelectedModel.PersonRequest, "List not sorted by lastupdated");
            }

        }

        private IPersonRequest CreatepersonRequestLastUpdated(DateTime dateTime)
        {
            IPersonRequest personRequest = CreateRequestObject(_person, _period);
            ReflectionHelper.SetUpdatedOn(personRequest, dateTime);
            return personRequest;
        }

        [Test]
        public void VerifyValidatesAgainstSchedulePeriod()
        {
            _target = new HandlePersonRequestViewModel(new DateTimePeriod(DateTime.MinValue.ToUniversalTime(), DateTime.MaxValue.ToUniversalTime()),
                new List<IPerson> { _person }, new UndoRedoContainer(), _allAccounts, _eventAggregator, new PersonRequestAuthorizationCheckerForTest(), _timeZoneInfo);

            IList<IPersonRequest> requests = new List<IPersonRequest> { _source };
            _target.CreatePersonRequestViewModels(requests, new ShiftTradeRequestStatusCheckerForTestDoesNothing(), new PersonRequestAuthorizationCheckerForTest());

            _target.PersonRequestViewModels.MoveCurrentToFirst();
            Assert.IsTrue(_target.SelectedModel.IsWithinSchedulePeriod, "The model is within the period, so its set to true");

            _target = new HandlePersonRequestViewModel(new DateTimePeriod(DateTime.MinValue.ToUniversalTime(), DateTime.MinValue.AddDays(2).ToUniversalTime()),
                new List<IPerson> { _person }, new UndoRedoContainer(), _allAccounts, _eventAggregator, new PersonRequestAuthorizationCheckerForTest(), _timeZoneInfo);
            _target.CreatePersonRequestViewModels(requests, new ShiftTradeRequestStatusCheckerForTestDoesNothing(), new PersonRequestAuthorizationCheckerForTest());
            _target.PersonRequestViewModels.MoveCurrentToFirst();
            Assert.IsFalse(_target.SelectedModel.IsWithinSchedulePeriod, "Model is outside the period, so it should be set to false when created");
        }

        [Test]
        public void VerifyValidatesAgainstSchedulePeriodAndPerson()
        {
            _target = new HandlePersonRequestViewModel(new DateTimePeriod(DateTime.MinValue.ToUniversalTime(), DateTime.MaxValue.ToUniversalTime()),
                new List<IPerson> { _person }, new UndoRedoContainer(), _allAccounts, _eventAggregator, new PersonRequestAuthorizationCheckerForTest(), _timeZoneInfo);

            IList<IPersonRequest> requests = new List<IPersonRequest> { _source };
            _target.CreatePersonRequestViewModels(requests, new ShiftTradeRequestStatusCheckerForTestDoesNothing(), new PersonRequestAuthorizationCheckerForTest());

            _target.PersonRequestViewModels.MoveCurrentToFirst();
            Assert.IsTrue(_target.SelectedModel.IsWithinSchedulePeriod, "The model is within the period, so its set to true");

            _target = new HandlePersonRequestViewModel(new DateTimePeriod(DateTime.MinValue.ToUniversalTime(), DateTime.MaxValue.ToUniversalTime()),
                new List<IPerson> { PersonFactory.CreatePerson("Tommy") }, new UndoRedoContainer(), _allAccounts, _eventAggregator, new PersonRequestAuthorizationCheckerForTest(), _timeZoneInfo);
            _target.CreatePersonRequestViewModels(requests, new ShiftTradeRequestStatusCheckerForTestDoesNothing(), new PersonRequestAuthorizationCheckerForTest());
            _target.PersonRequestViewModels.MoveCurrentToFirst();
            Assert.IsFalse(_target.SelectedModel.IsWithinSchedulePeriod, "Model is within the period but purson is not within, so it should be set to false when created");
        }
		
        [Test]
        public void ShouldDeletePersonRequestViewModelFromCollection()
        {
			_target.CreatePersonRequestViewModels(new List<IPersonRequest> { _source.WithId() }, _shiftTradeRequestStatusChecker, new PersonRequestAuthorizationCheckerForTest());

			_target.PersonRequestViewModels.Count.Should().Be.EqualTo(1);
            _target.DeletePersonRequestViewModel(_source);
            _target.PersonRequestViewModels.Count.Should().Be.EqualTo(0);
        }

        [Test]
        public void ShouldNotDeletePersonRequestViewModelFromCollectionIfNotFound()
        {
			_target.CreatePersonRequestViewModels(new List<IPersonRequest> { _source.WithId() }, _shiftTradeRequestStatusChecker, new PersonRequestAuthorizationCheckerForTest());

			_target.PersonRequestViewModels.Count.Should().Be.EqualTo(1);
			_target.DeletePersonRequestViewModel(_source.WithId());
			_target.PersonRequestViewModels.Count.Should().Be.EqualTo(0);
		}
		
        [Test]
        public void VerifyFilterIsCreated()
        {
            Assert.IsNotNull(_target.PersonRequestViewModels.Filter);
        }

        [Test]
        public void VerifyNewlyAddedItemsDoNotGetFiltered()
        {
            //Its not so often, but just incase, if somebody ads a new request, this will be shown even if the filter is active
            IList<IPersonRequest> requests = new List<IPersonRequest> { _source };
            _target.CreatePersonRequestViewModels(requests, _shiftTradeRequestStatusChecker, new PersonRequestAuthorizationCheckerForTest());

            Assert.AreEqual(_target.PersonRequestViewModels.Count, 1);
            _target.ShowOnly(GetAllModels());

            //They should still be visible:
            Assert.AreEqual(_target.PersonRequestViewModels.Count, 1);

            //Add another model:
            _target.InsertPersonRequestViewModel(_source,_shiftTradeRequestStatusChecker,new PersonRequestAuthorizationCheckerForTest());
            Assert.AreEqual(_target.PersonRequestViewModels.Count, 2, "The added model should be shown");

        }

        [Test]
        public void VerifyShowOnly()
        {
            _target.CreatePersonRequestViewModels(new List<IPersonRequest> {_source,_source,_source,_source},_shiftTradeRequestStatusChecker,new PersonRequestAuthorizationCheckerForTest());

	        _target.PersonRequestViewModels.MoveCurrentToPosition(0);
	        var adapter1 = (PersonRequestViewModel) _target.PersonRequestViewModels.CurrentItem;
			_target.PersonRequestViewModels.MoveCurrentToPosition(2);
			var adapter3 = (PersonRequestViewModel) _target.PersonRequestViewModels.CurrentItem;

            Assert.AreEqual(_target.PersonRequestViewModels.Count, 4);
            IList<PersonRequestViewModel> list = new List<PersonRequestViewModel> { adapter1, adapter3 };

            _target.ShowOnly(list);
            //Pick some and filter by

            Assert.AreEqual(_target.PersonRequestViewModels.Count, 2);
            Assert.IsTrue(_target.PersonRequestViewModels.Contains(adapter1));
            Assert.IsTrue(_target.PersonRequestViewModels.Contains(adapter3));
            //Clear filter,
            _target.ShowAll();
            Assert.AreEqual(_target.PersonRequestViewModels.Count, 4);
        }
		
        private static IPersonRequest CreateRequestObject(IPerson person, DateTimePeriod period)
        {
            IAbsence absence = AbsenceFactory.CreateAbsence("absence");
            IAbsenceRequest part = new AbsenceRequest(absence, period);
            IPersonRequest request = new PersonRequest(person, part);
            request.Pending();
            request.Approve(new ApprovalServiceForTest(), new PersonRequestAuthorizationCheckerForTest());
            return request;
        }
		
        private IList<PersonRequestViewModel> GetAllModels()
        {
            return (IList<PersonRequestViewModel>)_target.PersonRequestViewModels.SourceCollection;
        }
    }
}