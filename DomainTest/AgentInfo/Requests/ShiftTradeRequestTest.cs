using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
    /// <summary>
    /// Tests for ShiftTradeRequest
    /// </summary>
    [TestFixture]
    public class ShiftTradeRequestTest
    {
        private ShiftTradeRequest _target;
        private IPerson _requestedPerson;
        private IPersonRequest _personRequest;
        private IShiftTradeSwapDetail _shiftTradeSwapDetail;
        private IPerson _tradePerson;
        private IPersonRequestCheckAuthorization _authorization;
        private const string ShiftTradeRequestHasBeenDeniedDot = "ShiftTradeRequestHasBeenDeniedDot";
        private const string ShiftTradeRequestHasBeenApprovedDot = "ShiftTradeRequestHasBeenApprovedDot";
        private const string ShiftTradeRequestHasBeenAcceptedDot = "ShiftTradeRequestHasBeenAcceptedDot";
        private const string ShiftTradeRequestHasBeenReferredDot= "ShiftTradeRequestHasBeenReferredDot";
        private const string ANewShiftTradeHasBeenCreatedDot = "ANewShiftTradeHasBeenCreatedDot";

        [SetUp]
        public void Setup()
        {
            _requestedPerson = PersonFactory.CreatePerson("Mama","Hawa");
            _tradePerson = PersonFactory.CreatePerson("Day", "Trader");
            _shiftTradeSwapDetail = new ShiftTradeSwapDetail(_requestedPerson, _tradePerson,
                                                             new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16));

            _authorization = new PersonRequestAuthorizationCheckerForTest();
            _target = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>{_shiftTradeSwapDetail});

            _personRequest = new PersonRequest(_requestedPerson);
            _personRequest.Request = _target;
        }

        [Test]
        public void VerifyCanGetInvolvedPeople()
        {
            var involvedPeople = _target.InvolvedPeople();
            Assert.AreEqual(2,involvedPeople.Count());
        }

        /// <summary>
        /// Verifies the instance created.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-19
        /// </remarks>
        [Test]
        public void VerifyInstanceCreated()
        {
            Assert.IsNotNull(_target);
        }

        /// <summary>
        /// Verifies the has empty constructor.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-19
        /// </remarks>
        [Test]
        public void VerifyHasEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType()));
        }

        /// <summary>
        /// Verifies the properties.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-19
        /// </remarks>
        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(
                new DateOnlyPeriod(_shiftTradeSwapDetail.DateFrom, _shiftTradeSwapDetail.DateFrom).ToDateTimePeriod(
                    _target.Person.PermissionInformation.DefaultTimeZone()), _target.Period);
            Assert.AreEqual(ShiftTradeStatus.OkByMe, _target.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));

            _target.RequestTypeDescription = "ShiftTrade";
            Assert.AreEqual("ShiftTrade", _target.RequestTypeDescription);

            Assert.AreEqual(new Description().Name, _target.RequestPayloadDescription.Name);
        }

        [Test]
        public void VerifyDeny()
        {
            _target.Deny(_tradePerson);
            Assert.IsNotNull(_target);
            
            Assert.IsTrue(MessageWillOnlyBeSentToRequestPerson(),  "Only Request person needs to be notified");
            Assert.AreEqual(ShiftTradeRequestHasBeenDeniedDot, _target.TextForNotification);
        }

        [Test]
        public void VerifyShiftTradeStatusCanSet()
        {
            Assert.AreEqual(ShiftTradeStatus.OkByMe, _target.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
            _target.SetShiftTradeStatus(ShiftTradeStatus.OkByBothParts,_authorization);
            Assert.AreEqual(ShiftTradeStatus.OkByBothParts, _target.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));

            _target.SetShiftTradeStatus(ShiftTradeStatus.NotValid,_authorization);
            Assert.AreEqual(ShiftTradeStatus.NotValid, _target.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
            Assert.IsTrue(_personRequest.IsDenied);
        }

        [Test]
        public void VerifyApproveCallWorks()
        {
            MockRepository mocks = new MockRepository();
            IRequestApprovalService requestApprovalService =
                mocks.StrictMock<IRequestApprovalService>();
            PersonRequest personRequest = new PersonRequest(_requestedPerson, _target);

            Expect.Call(requestApprovalService.ApproveShiftTrade(null)).Return(new List<IBusinessRuleResponse>()).IgnoreArguments();

            mocks.ReplayAll();

            personRequest.Pending();
            IList<IBusinessRuleResponse> brokenRules = personRequest.Approve(requestApprovalService,_authorization);
            Assert.AreEqual(0, brokenRules.Count);
            Assert.AreEqual(ShiftTradeRequestHasBeenApprovedDot, _target.TextForNotification);
            Assert.IsTrue(MessageWillBeSentToBothPersons(), "Message should be sent to both persons when approving");

            mocks.VerifyAll();
        }

        [Test]
        public void VerifyApproveCallWorksWithoutNotificationIfValidationError()
        {
            MockRepository mocks = new MockRepository();
            IRequestApprovalService requestApprovalService =
                mocks.StrictMock<IRequestApprovalService>();
            PersonRequest personRequest = new PersonRequest(_requestedPerson, _target);

            Expect.Call(requestApprovalService.ApproveShiftTrade(null)).Return(new List<IBusinessRuleResponse>{null}).IgnoreArguments();

            mocks.ReplayAll();

            personRequest.Pending();
            IList<IBusinessRuleResponse> brokenRules = personRequest.Approve(requestApprovalService,_authorization);
            Assert.AreEqual(1, brokenRules.Count);
            Assert.AreEqual(ANewShiftTradeHasBeenCreatedDot, _target.TextForNotification);

            mocks.VerifyAll();
        }

        [Test]
        public void CanAccept()
        {
            MockRepository mocks = new MockRepository();
            IShiftTradeRequestSetChecksum shiftTradeRequestSetChecksum =
                mocks.StrictMock<IShiftTradeRequestSetChecksum>();
            shiftTradeRequestSetChecksum.SetChecksum(_target);
            LastCall.Repeat.Once();

            mocks.ReplayAll();
            ShiftTradeRequestStatusCheckerForTestDoesNothing shiftTradeRequestStatusCheckerForTestDoesNothing =
                new ShiftTradeRequestStatusCheckerForTestDoesNothing();
            _target.Refer(_authorization);
            Assert.AreEqual(ShiftTradeStatus.Referred, _target.GetShiftTradeStatus(shiftTradeRequestStatusCheckerForTestDoesNothing));
            _target.Accept(_requestedPerson, shiftTradeRequestSetChecksum,_authorization);
            Assert.IsTrue(MessageWillOnlyBeSentToTradePerson(), "TradePerson (only) should be notified when accepted from requestperson");
            
            Assert.AreEqual(ShiftTradeStatus.OkByMe, _target.GetShiftTradeStatus(shiftTradeRequestStatusCheckerForTestDoesNothing));
            _target.Accept(_tradePerson, shiftTradeRequestSetChecksum,_authorization);
           
            Assert.AreEqual(ShiftTradeStatus.OkByBothParts, _target.GetShiftTradeStatus(shiftTradeRequestStatusCheckerForTestDoesNothing));
         
            Assert.IsTrue(MessageWillOnlyBeSentToRequestPerson(),"RequestPerson (only) should be notified when accepted from targetperson");

            Assert.AreEqual(ShiftTradeRequestHasBeenAcceptedDot, _target.TextForNotification);
            mocks.VerifyAll();
        }

        [Test,ExpectedException(typeof(ArgumentNullException))]
        public void VerifyMustSupplyPersonToAccept()
        {
            _target.Accept(null,new EmptyShiftTradeRequestSetChecksum(),_authorization);
        }

        /// <summary>
        /// Determines whether this instance can refer.
        /// </summary>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2009-08-31
        /// </remarks>
        [Test]
        public void CanRefer()
        {
            Assert.AreEqual(ShiftTradeStatus.OkByMe, _target.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
            _target.Refer(_authorization);
            Assert.AreEqual(ShiftTradeStatus.Referred, _target.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
            Assert.IsTrue(_personRequest.IsPending);
            Assert.AreEqual(ShiftTradeRequestHasBeenReferredDot, _target.TextForNotification);
        }

        [Test]
        public void VerifyCanAddAndClearShiftTradeSwapDetails()
        {
            DateOnly dateOnly = new DateOnly(2009, 9, 3);
            Assert.AreNotEqual(new DateTimePeriod(),_target.Period);
            Assert.AreNotEqual(0,_target.ShiftTradeSwapDetails.Count);
            _target.ClearShiftTradeSwapDetails();
            Assert.AreEqual(0,_target.ShiftTradeSwapDetails.Count);
            Assert.AreEqual(new DateTimePeriod(), _target.Period);
            _target.AddShiftTradeSwapDetail(new ShiftTradeSwapDetail(_requestedPerson,_requestedPerson,dateOnly,dateOnly));
            Assert.AreEqual(1,_target.ShiftTradeSwapDetails.Count);
            Assert.AreEqual(_target,_target.ShiftTradeSwapDetails[0].Parent);
            Assert.AreEqual(
                new DateOnlyPeriod(dateOnly, dateOnly).ToDateTimePeriod(
                    _target.Person.PermissionInformation.DefaultTimeZone()), _target.Period);
        }
        [Test]
        public void VerifyCanGetDetails()
        {
            string text = _target.GetDetails(new CultureInfo("en-US"));

            Assert.AreEqual("Mama Hawa, Day Trader, 7/16/2008", text);
            text = _target.GetDetails(new CultureInfo("ko-KR"));
            Assert.AreEqual("Mama Hawa, Day Trader, 2008-07-16", text);
            text = _target.GetDetails(new CultureInfo("zh-TW"));
            Assert.AreEqual("Mama Hawa, Day Trader, 2008/7/16", text);
        }

        [Test, ExpectedException(typeof(ShiftTradeRequestStatusException))]
        public void VerifyCannotGoFromReferredToOkByBothParts()
        {
            _target.Refer(_authorization);
            _target.SetShiftTradeStatus(ShiftTradeStatus.OkByBothParts,_authorization);
        }

        [Test, ExpectedException(typeof(ShiftTradeRequestStatusException))]
        public void VerifyCannotGoFromReferredWhenAccepting()
        {
            _target.Refer(_authorization);
            _target.Accept(_tradePerson,new EmptyShiftTradeRequestSetChecksum(),_authorization);
        }

        [Test, ExpectedException(typeof(ShiftTradeRequestStatusException))]
        public void VerifyCannotAddShiftTradeSwapDetailsWhenOkByBothParts()
        {
            _target.Accept(_tradePerson,new EmptyShiftTradeRequestSetChecksum(),_authorization);
            _target.AddShiftTradeSwapDetail(new ShiftTradeSwapDetail(_requestedPerson,_requestedPerson,_shiftTradeSwapDetail.DateFrom,_shiftTradeSwapDetail.DateTo));
        }

        [Test, ExpectedException(typeof(ShiftTradeRequestStatusException))]
        public void VerifyCannotAddShiftTradeSwapDetailsWhenNotValid()
        {
            _target.SetShiftTradeStatus(ShiftTradeStatus.NotValid,_authorization);
            _target.AddShiftTradeSwapDetail(new ShiftTradeSwapDetail(_requestedPerson, _requestedPerson, _shiftTradeSwapDetail.DateFrom, _shiftTradeSwapDetail.DateTo));
        }

        [Test, ExpectedException(typeof(ShiftTradeRequestStatusException))]
        public void VerifyCannotClearShiftTradeSwapDetailsWhenOkByBothParts()
        {
            _target.Accept(_tradePerson,new EmptyShiftTradeRequestSetChecksum(),_authorization);
            _target.ClearShiftTradeSwapDetails();
        }

        [Test, ExpectedException(typeof(ShiftTradeRequestStatusException))]
        public void VerifyCannotClearShiftTradeSwapDetailsWhenNotValid()
        {
            _target.SetShiftTradeStatus(ShiftTradeStatus.NotValid,_authorization);
            _target.ClearShiftTradeSwapDetails();
        }

        [Test]
        public void VerifyPushMessageIsCreatedForToPersonWhenCreatingAShiftTradeRequest()
        {
            Assert.IsTrue(MessageWillBeSentToTradePerson());
            Assert.AreEqual(_target.ReceiversForNotification.Count,1,"Only the person involved in the trade should be notified");
        }

        [Test]
        public void VerifyPushMessageIsCreatedForToPersonWhenAddingShiftTradeRequest()
        {
            IPerson anotherPerson = PersonFactory.CreatePerson("AnotherPerson");
            _shiftTradeSwapDetail = new ShiftTradeSwapDetail(_requestedPerson, anotherPerson,
                                                              new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16));

            _target = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { _shiftTradeSwapDetail });
            Assert.IsTrue(_target.ShouldNotifyWithMessage, "When a shifttrade is created,a pushmessage should be generated to the person thats involved in the trade");
            Assert.AreEqual(anotherPerson, _target.ReceiversForNotification.First(), "When a shifttrade is created,a pushmessage should be generated to the person thats involved in the trade");
            Assert.AreEqual(_target.ReceiversForNotification.Count, 1, "Only the person involved in the trade should be notified");
        }

        [Test]
        public void ShouldSendNotificationToOtherPersonManually()
        {
            IPerson anotherPerson = PersonFactory.CreatePerson("AnotherPerson");
            _shiftTradeSwapDetail = new ShiftTradeSwapDetail(_requestedPerson, anotherPerson,
                                                              new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16));

            _target = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { _shiftTradeSwapDetail });
            _target.TextForNotification = string.Empty;
            _target.ReceiversForNotification.Clear();

            _target.NotifyToPersonAfterValidation();
            Assert.IsTrue(_target.ShouldNotifyWithMessage, "When a shifttrade is created,a pushmessage should be generated to the person thats involved in the trade");
            Assert.AreEqual(anotherPerson, _target.ReceiversForNotification.First(), "When a shifttrade is created,a pushmessage should be generated to the person thats involved in the trade");
            Assert.AreEqual(_target.ReceiversForNotification.Count, 1, "Only the person involved in the trade should be notified");
        }

        private bool MessageWillBeSentToRequestPerson()
        {
            return _target.ReceiversForNotification.Contains(_requestedPerson) && _target.ShouldNotifyWithMessage;
        }

        private bool MessageWillBeSentToTradePerson()
        {
            return _target.ReceiversForNotification.Contains(_tradePerson) && _target.ShouldNotifyWithMessage;
        }

        private bool MessageWillOnlyBeSentToRequestPerson()
        {
            return _target.ReceiversForNotification.Count==1 && MessageWillBeSentToRequestPerson();
        }

        private bool MessageWillOnlyBeSentToTradePerson()
        {
            return _target.ReceiversForNotification.Count == 1 && MessageWillBeSentToTradePerson();
        }

        private bool MessageWillBeSentToBothPersons()
        {
            return MessageWillBeSentToRequestPerson() && MessageWillBeSentToTradePerson();
        }
    }
}
