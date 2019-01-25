using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCodeTest.Helpers;

using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class PersonsRequestViewModelTest
    {
        private PersonRequestViewModel _personRequestViewModel;
        private IPersonRequest _source;
        private IPerson _person;
        private readonly DateTimePeriod 
            _period = new DateTimePeriod(new DateTime(2000, 1, 1, 8, 45, 0, DateTimeKind.Utc),
                                         new DateTime(2001, 1, 1, 10, 45, 0, DateTimeKind.Utc));
        private IPersonPeriod _personPeriod;
        private AccountTime _personTime;
        private IShiftTradeRequestStatusChecker _shiftTradeRequestStatusChecker;
        private IPersonAccountCollection _personAccount;
        private EventAggregator _eventAggregator;
        private TimeZoneInfo _TimeZoneInfo;

        [SetUp]
        public void Setup()
        {
            IAbsence absence = AbsenceFactory.CreateAbsence("absence");
            absence.Tracker = Tracker.CreateTimeTracker();
            _personTime = new AccountTime(new DateOnly(2000, 1, 1));
            _personTime.Accrued = TimeSpan.FromHours(2.34);
            _personTime.BalanceIn = TimeSpan.FromHours(100);
            _person = PersonFactory.CreatePerson("First", "Last");
            _person.PermissionInformation.SetDefaultTimeZone(
								TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
            _personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2005, 1, 1), TeamFactory.CreateSimpleTeam());
            _person.AddPersonPeriod(_personPeriod);
            _eventAggregator = new EventAggregator();

            _source = CreateRequestObject(_person, _period, absence);
            _shiftTradeRequestStatusChecker = new ShiftTradeRequestStatusCheckerForTestDoesNothing();

            _personAccount = new PersonAccountCollection(_person);
            var gris = new PersonAbsenceAccount(_person, absence);
            gris.Add(_personTime);
            _personAccount.Add(gris);
            _TimeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("UTC"));
            _personRequestViewModel = new PersonRequestViewModel(_source, _shiftTradeRequestStatusChecker, _personAccount, _eventAggregator, _TimeZoneInfo);

            ReflectionHelper.SetUpdatedOn(_source,new DateTime(2009,12,22,23,0,0,DateTimeKind.Utc));
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsTrue(_personRequestViewModel.IsEditable);

            _source.TrySetMessage("message");
            _source.Subject = "subject";
            _personRequestViewModel = new PersonRequestViewModel(_source, _shiftTradeRequestStatusChecker, _personAccount, null, _TimeZoneInfo);

            Assert.AreEqual(_source.Request.Period.StartDateTime.ToShortDateString() + " - " + _source.Request.Period.EndDateTime.ToShortDateString(), _personRequestViewModel.RequestedDate);
            Assert.AreEqual(_source.Person.Name.ToString(), _personRequestViewModel.Name);
            Assert.AreEqual(_source.Person.Seniority, _personRequestViewModel.Seniority);
            Assert.AreEqual(UserTexts.Resources.RequestTypeAbsence, _personRequestViewModel.RequestType);
            Assert.IsTrue(_personRequestViewModel.IsPending);
            Assert.AreEqual(_source.Request.GetDetails(CultureInfo.CurrentCulture), _personRequestViewModel.Details);
			Assert.AreEqual(_source.GetSubject(new NoFormatting()), _personRequestViewModel.GetSubject(new NoFormatting()));
			Assert.AreEqual(_source.GetMessage(new NoFormatting()), _personRequestViewModel.GetMessage(new NoFormatting()));
            Assert.AreEqual(_source.UpdatedOn, _personRequestViewModel.LastUpdated);
            Assert.AreEqual(_source.Request.Period.StartDateTime,_personRequestViewModel.FirstDateInRequest);
            Assert.IsFalse(_personRequestViewModel.CausesBrokenBusinessRule);
            _personRequestViewModel.CausesBrokenBusinessRule = true;
            Assert.IsTrue(_personRequestViewModel.CausesBrokenBusinessRule);
			var balance = string.Concat(TimeHelper.GetLongHourMinuteTimeString(_personTime.BalanceIn.Add(_personTime.Accrued), CultureInfo.CurrentCulture), " ",
					Resources.Hours);
			Assert.AreEqual(balance, _personRequestViewModel.Left);
        }

        [Test]
        public void CanRaiseEvent()
        {
            bool causesBrokenBusinessRuleChangedCalled = false;

            _personRequestViewModel = new PersonRequestViewModel(_source, _shiftTradeRequestStatusChecker, _personAccount, null, _TimeZoneInfo);
            _personRequestViewModel.CausesBrokenBusinessRuleChanged += (x,y) => {
                                                                                causesBrokenBusinessRuleChangedCalled =
                                                                                    true;};
            _personRequestViewModel.CausesBrokenBusinessRule = true;
            
            Assert.IsTrue(causesBrokenBusinessRuleChangedCalled);
        }

        [Test]
        public void VerifyIsWithinSchedulePeriod()
        {
            DateTime requestDateTime = _personRequestViewModel.PersonRequest.RequestedDate;
            TimeSpan diff = TimeSpan.FromDays(2);
            Assert.IsTrue(_personRequestViewModel.IsWithinSchedulePeriod,"Default true if no period is set");
            DateTimePeriod periodContaining = new DateTimePeriod(requestDateTime.Subtract(diff),
                                                                 requestDateTime.Add(diff));
            DateTimePeriod periodNotContaining = new DateTimePeriod(requestDateTime.Add(diff),requestDateTime.Add(diff).Add(diff));

            PropertyChangedListener listener = new PropertyChangedListener();
            listener.ListenTo(_personRequestViewModel);
            _personRequestViewModel.ValidateIfWithinSchedulePeriod(periodNotContaining, new List<IPerson>{_person});
            Assert.IsFalse(_personRequestViewModel.IsWithinSchedulePeriod);
            Assert.IsTrue(listener.HasFired("IsWithinSchedulePeriod"));

            listener.Clear();
            _personRequestViewModel.ValidateIfWithinSchedulePeriod(periodContaining, new List<IPerson>{_person});
            Assert.IsTrue(_personRequestViewModel.IsWithinSchedulePeriod);
            Assert.IsTrue(listener.HasFired("IsWithinSchedulePeriod"));
        }

        [Test]
        public void VerifyTradeShiftWithOtherStatusThanOkByBothParts()
        {
            const string firstName = "Kalle";
            const string lastName = "Kula";
            IPerson tradeWithPerson = PersonFactory.CreatePerson(firstName,lastName);

            IPersonRequest shiftTradePersonRequest =
                CreateShiftTradeRequestObject(
                _person,
                tradeWithPerson,
                new List<DateOnly>{
                    DateOnly.Today.AddDays(1),
                    DateOnly.Today.AddDays(2),
                    DateOnly.Today.AddDays(3)});

            PersonRequestViewModel shiftTradePersonRequestViewModel =
                new PersonRequestViewModel(shiftTradePersonRequest, _shiftTradeRequestStatusChecker, _personAccount, null, _TimeZoneInfo);
            Assert.IsFalse(shiftTradePersonRequestViewModel.IsEditable);
            ((IShiftTradeRequest)shiftTradePersonRequest.Request).Accept(tradeWithPerson, new EmptyShiftTradeRequestSetChecksum(), new PersonRequestAuthorizationCheckerForTest());
            Assert.IsTrue(shiftTradePersonRequestViewModel.IsEditable);
            ((IShiftTradeRequest)shiftTradePersonRequest.Request).Refer(new PersonRequestAuthorizationCheckerForTest());
            Assert.IsFalse(shiftTradePersonRequestViewModel.IsEditable);
        }

        [Test]
        public void ShouldOnlyExposeOneDateIfOneDateOnlyInShiftTrade()
        {
            const string firstName = "Kalle";
            const string lastName = "Kula";
            IPerson tradeWithPerson = PersonFactory.CreatePerson(firstName, lastName);

            IPersonRequest shiftTradePersonRequest =
                CreateShiftTradeRequestObject(
                _person,
                tradeWithPerson,
                new List<DateOnly>{
                    DateOnly.Today.AddDays(1)});

            PersonRequestViewModel shiftTradePersonRequestViewModel =
                new PersonRequestViewModel(shiftTradePersonRequest, _shiftTradeRequestStatusChecker, _personAccount, null, _TimeZoneInfo);
            Assert.AreEqual(DateOnly.Today.AddDays(1).ToShortDateString(),shiftTradePersonRequestViewModel.RequestedDate);
        }

        [Test]
        public void ShouldContainMultipleIfMultipleDaysShiftTrade()
        {
            const string firstName = "Kalle";
            const string lastName = "Kula";
            IPerson tradeWithPerson = PersonFactory.CreatePerson(firstName, lastName);

            IPersonRequest shiftTradePersonRequest =
                CreateShiftTradeRequestObject(
                _person,
                tradeWithPerson,
                new List<DateOnly>{
                    DateOnly.Today.AddDays(1),
                    DateOnly.Today.AddDays(2)});

            PersonRequestViewModel shiftTradePersonRequestViewModel =
                new PersonRequestViewModel(shiftTradePersonRequest, _shiftTradeRequestStatusChecker, _personAccount, null, _TimeZoneInfo);
            Assert.IsTrue(shiftTradePersonRequestViewModel.RequestedDate.Contains(UserTexts.Resources.MultipleValuesParanteses));
        }

        [Test]
        public void VerifyIsSelectedEvents()
        {
            PersonRequestViewModel personRequestViewModelFromEvent=null;
            _eventAggregator.GetEvent<GenericEvent<PersonRequestViewModelIsSelectedChanged>>().Subscribe(e => personRequestViewModelFromEvent = e.Value.Model);
            
            PropertyChangedListener listener = new PropertyChangedListener();
            listener.ListenTo(_personRequestViewModel);
            Assert.IsFalse(_personRequestViewModel.IsSelected);
            Assert.IsNull(personRequestViewModelFromEvent);
            
            _personRequestViewModel.IsSelected = true;
            
            Assert.IsTrue(_personRequestViewModel.IsSelected,"Verify the property has changed");
          //  Assert.IsTrue(listener.HasOnlyFired("IsSelected"),"Verify INotifypropertyChanged has fired");
            Assert.AreEqual(personRequestViewModelFromEvent,_personRequestViewModel);
        }

        [Test]
        public void VerifyStatusChangeNotifiesGui()
        {
            const string denyReason = "RequestDeniedByAdministrator";
            PropertyChangedListener listener = new PropertyChangedListener();
            listener.ListenTo(_personRequestViewModel);
            Assert.IsFalse(_personRequestViewModel.IsDenied);
            _personRequestViewModel.PersonRequest.Deny(denyReason, new PersonRequestAuthorizationCheckerForTest());
            Assert.IsTrue(_personRequestViewModel.IsDenied);
            Assert.AreEqual(denyReason, _personRequestViewModel.PersonRequest.DenyReason);
            Assert.IsTrue(listener.HasFired("StatusText"));
        }

		[Test]
		public void VerifyOvertimeRequestType()
		{
			var overtimeRequest = new OvertimeRequest(new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime), _period);
			var personRequest = new PersonRequest(_person, overtimeRequest);
			personRequest.Pending();
			_personRequestViewModel = new PersonRequestViewModel(personRequest, _shiftTradeRequestStatusChecker, _personAccount, null, _TimeZoneInfo);

			Assert.AreEqual(UserTexts.Resources.RequestTypeOvertime, _personRequestViewModel.RequestType);
		}

		#region helpers
		private static IPersonRequest CreateRequestObject(IPerson person, DateTimePeriod period, IAbsence absence)
        {
            
            IAbsenceRequest part = new AbsenceRequest(absence, period);
            var request = new PersonRequest(person, part);
            request.Pending();
            return request;
        }

        private static IPersonRequest CreateShiftTradeRequestObject(IPerson tradingPerson, IPerson tradeWithPerson, IList<DateOnly> period)
        {
            IList<IShiftTradeSwapDetail> shiftTradeSwapDetails = new List<IShiftTradeSwapDetail>();
            foreach (DateOnly dateOnly in period)
            {
                shiftTradeSwapDetails.Add(new ShiftTradeSwapDetail(tradingPerson, tradeWithPerson, dateOnly, dateOnly));
            }
            IShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(shiftTradeSwapDetails);
            var request = new PersonRequest(tradingPerson, shiftTradeRequest);
            request.Pending();
            return request;
        }

        #endregion //helpers
    }
}
