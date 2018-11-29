using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.UserTexts;


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
       

        [SetUp]
        public void Setup()
        {
            _requestedPerson = PersonFactory.CreatePerson("Mama","Hawa");
            _tradePerson = PersonFactory.CreatePerson("Day", "Trader");

			CultureInfo englishCulture = CultureInfoFactory.CreateEnglishCulture();
			setPersonCulture(_requestedPerson, englishCulture);
			setPersonCulture(_tradePerson, englishCulture);
			setPersonLanguage(_requestedPerson, englishCulture);
			setPersonLanguage(_tradePerson, englishCulture);

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
                    _target.Person.PermissionInformation.DefaultTimeZone()).ChangeEndTime (new TimeSpan(0,-1,0)), _target.Period);
            Assert.AreEqual(ShiftTradeStatus.OkByMe, _target.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));

            _target.RequestTypeDescription = "ShiftTrade";
            Assert.AreEqual("ShiftTrade", _target.RequestTypeDescription);

            Assert.AreEqual(new Description().Name, _target.RequestPayloadDescription.Name);
        }

        [Test]
		public void ShouldDenyMessageForTranslatedCultureBeRequestPersonOwnLanguage()
        {

			setPersonCulture(_requestedPerson, CultureInfoFactory.CreateSwedishCulture());
			setPersonLanguage(_requestedPerson, CultureInfoFactory.CreateSwedishCulture());
		
			_target.Deny(_tradePerson);
            Assert.IsNotNull(_target);
            
            Assert.IsTrue(messageWillOnlyBeSentToRequestPerson(),  "Only Request person needs to be notified");

	        var notificationString = "En skiftbytesförfrågan 2008-07-16 nekades.";
	        Assert.AreEqual(notificationString, _target.TextForNotification);
        }

		[Test, SetCulture("sv-SE")]
		public void ShouldDenyMessageForNotTranslatedCultureBeInEnglish()
		{

			setPersonCulture(_requestedPerson, CultureInfoFactory.CreateCatalanCulture());
			setPersonLanguage(_requestedPerson, CultureInfoFactory.CreateCatalanCulture());

			_target.Deny(_tradePerson);
			Assert.IsNotNull(_target);

			Assert.IsTrue(messageWillOnlyBeSentToRequestPerson(), "Only Request person needs to be notified");

			var timeZone = _requestedPerson.PermissionInformation.DefaultTimeZone();
			var format = _requestedPerson.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern;
			var notificationString = string.Format("A shift trade request {0} was denied."
				, _personRequest.Request.Period.StartDateTimeLocal(timeZone).ToString(format));

			Assert.AreEqual(notificationString, _target.TextForNotification);
		}

	    [Test]
	    public void ShouldSetOfferStatusToPendingWhenDeny()
	    {
			 var offer = MockRepository.GenerateMock<IShiftExchangeOffer>();
		    offer.Status = ShiftExchangeOfferStatus.PendingAdminApproval;
		    _target.Offer = offer;
		    var expectStatus = ShiftExchangeOfferStatus.Pending;
		    offer.Stub(x => x.Status).Return(expectStatus);

			 _target.Deny(_tradePerson);

			 _target.Offer.Status.Should().Be.EqualTo(expectStatus);
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

        [Test, SetCulture("sv-SE")]
        public void VerifyApproveCallWorks()
        {
            MockRepository mocks = new MockRepository();
            IRequestApprovalService requestApprovalService =
                mocks.StrictMock<IRequestApprovalService>();
            PersonRequest personRequest = new PersonRequest(_requestedPerson, _target);

            Expect.Call(requestApprovalService.Approve(null)).Return(new List<IBusinessRuleResponse>()).IgnoreArguments();

            mocks.ReplayAll();

            personRequest.Pending();
            IList<IBusinessRuleResponse> brokenRules = personRequest.Approve(requestApprovalService,_authorization);
            Assert.AreEqual(0, brokenRules.Count);
	        var notificationString = "A shift trade request 16-07-2008 was approved.";

            Assert.AreEqual(notificationString, _target.TextForNotification);
            Assert.IsTrue(messageWillBeSentToBothPersons(), "Message should be sent to both persons when approving");

            mocks.VerifyAll();
        }

        [Test, SetCulture("sv-SE")]
        public void VerifyApproveCallWorksWithoutNotificationIfValidationError()
        {
            MockRepository mocks = new MockRepository();
            IRequestApprovalService requestApprovalService =
                mocks.StrictMock<IRequestApprovalService>();
            PersonRequest personRequest = new PersonRequest(_requestedPerson, _target);

            Expect.Call(requestApprovalService.Approve(null)).Return(new List<IBusinessRuleResponse>{null}).IgnoreArguments();

            mocks.ReplayAll();

            personRequest.Pending();
            IList<IBusinessRuleResponse> brokenRules = personRequest.Approve(requestApprovalService,_authorization);
            Assert.AreEqual(1, brokenRules.Count);

			var notificationString = "New shift trade request 16-07-2008, approve or deny in your request list.";
            Assert.AreEqual(notificationString, _target.TextForNotification);

            mocks.VerifyAll();
        }

        [Test, SetCulture("sv-SE")]
        public void VerifyAccept()
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
            Assert.IsTrue(messageWillOnlyBeSentToTradePerson(), "TradePerson (only) should be notified when accepted from requestperson");
            
            Assert.AreEqual(ShiftTradeStatus.OkByMe, _target.GetShiftTradeStatus(shiftTradeRequestStatusCheckerForTestDoesNothing));
            _target.Accept(_tradePerson, shiftTradeRequestSetChecksum,_authorization);
           
            Assert.AreEqual(ShiftTradeStatus.OkByBothParts, _target.GetShiftTradeStatus(shiftTradeRequestStatusCheckerForTestDoesNothing));
         
            Assert.IsTrue(messageWillOnlyBeSentToRequestPerson(),"RequestPerson (only) should be notified when accepted from targetperson");

			var notificationString = "A shift trade request 16-07-2008 was accepted by other person.";

            Assert.AreEqual(notificationString, _target.TextForNotification);
            mocks.VerifyAll();
        }

		[Test]
		public void ShouldAcceptMessageForTranslatedCultureBeRequestPersonOwnLanguage()
		{

			setPersonCulture(_requestedPerson, CultureInfoFactory.CreateSwedishCulture());
			setPersonLanguage(_requestedPerson, CultureInfoFactory.CreateSwedishCulture());

			MockRepository mocks = new MockRepository();
			IShiftTradeRequestSetChecksum shiftTradeRequestSetChecksum =
				mocks.StrictMock<IShiftTradeRequestSetChecksum>();
			shiftTradeRequestSetChecksum.SetChecksum(_target);
			LastCall.Repeat.Once();

			mocks.ReplayAll();

			_target.Refer(_authorization);
			_target.Accept(_requestedPerson, shiftTradeRequestSetChecksum, _authorization);
			_target.Accept(_tradePerson, shiftTradeRequestSetChecksum, _authorization);

			// must be on the request person's own language with the culture's date format
			var notificationString = "En skiftbytesförfrågan 2008-07-16 accepterades av den andra personen.";

			Assert.AreEqual(notificationString, _target.TextForNotification);

			mocks.VerifyAll();
		}

	    [Test, SetCulture("sv-SE")]
		public void ShouldAcceptMessageForNotTranslatedCultureBeInEnglish()
		{

			setPersonCulture(_requestedPerson, CultureInfoFactory.CreateCatalanCulture());
			setPersonLanguage(_requestedPerson, CultureInfoFactory.CreateCatalanCulture());

			MockRepository mocks = new MockRepository();
			IShiftTradeRequestSetChecksum shiftTradeRequestSetChecksum =
				mocks.StrictMock<IShiftTradeRequestSetChecksum>();
			shiftTradeRequestSetChecksum.SetChecksum(_target);
			LastCall.Repeat.Once();

			mocks.ReplayAll();

			_target.Refer(_authorization);
			_target.Accept(_requestedPerson, shiftTradeRequestSetChecksum, _authorization);
			_target.Accept(_tradePerson, shiftTradeRequestSetChecksum, _authorization);

			var timeZone = _requestedPerson.PermissionInformation.DefaultTimeZone();
			var format = _requestedPerson.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern;
		    var notificationString = string.Format("A shift trade request {0} was accepted by other person.",
			    _personRequest.Request.Period.StartDateTimeLocal(timeZone).ToString(format));

			Assert.AreEqual(notificationString, _target.TextForNotification);

			mocks.VerifyAll();
		}

        [Test]
        public void VerifyMustSupplyPersonToAccept()
        {
            Assert.Throws<ArgumentNullException>(() => _target.Accept(null,new EmptyShiftTradeRequestSetChecksum(),_authorization));
        }

        [Test, SetCulture("sv-SE")]
        public void CanRefer()
        {
            Assert.AreEqual(ShiftTradeStatus.OkByMe, _target.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
            _target.Refer(_authorization);
            Assert.AreEqual(ShiftTradeStatus.Referred, _target.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
            Assert.IsTrue(_personRequest.IsPending);
			var notificationString =
				string.Format(Resources.ShiftTradeRequestForOneDayHasBeenReferredDot, "16-07-2008 - 16-07-2008");
            Assert.AreEqual(notificationString, _target.TextForNotification);
        }

		[Test]
		public void ShouldReferMessageForTranslatedCultureBeRequestPersonOwnLanguage()
		{

			setPersonCulture(_requestedPerson, CultureInfoFactory.CreateSwedishCulture());
			setPersonLanguage(_requestedPerson, CultureInfoFactory.CreateSwedishCulture());

			Assert.AreEqual(ShiftTradeStatus.OkByMe, _target.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
			_target.Refer(_authorization);
			Assert.AreEqual(ShiftTradeStatus.Referred, _target.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
			Assert.IsTrue(_personRequest.IsPending);
			
			var notificationString = "En schemaändring har gjort att en förfrågan om skiftbyte 2008-07-16 - 2008-07-16 måste accepteras igen.";
			
			Assert.AreEqual(notificationString, _target.TextForNotification);
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
                    _target.Person.PermissionInformation.DefaultTimeZone()).ChangeEndTime (new TimeSpan(0,-1,0)), _target.Period);
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

        [Test]
        public void VerifyCannotGoFromReferredToOkByBothParts()
        {
            _target.Refer(_authorization);
			Assert.Throws<ShiftTradeRequestStatusException>(() => _target.SetShiftTradeStatus(ShiftTradeStatus.OkByBothParts,_authorization));
        }

        [Test]
        public void VerifyCannotGoFromReferredWhenAccepting()
        {
            _target.Refer(_authorization);
			Assert.Throws<ShiftTradeRequestStatusException>(() => _target.Accept(_tradePerson,new EmptyShiftTradeRequestSetChecksum(),_authorization));
        }

        [Test]
        public void VerifyCannotAddShiftTradeSwapDetailsWhenOkByBothParts()
        {
            _target.Accept(_tradePerson,new EmptyShiftTradeRequestSetChecksum(),_authorization);
			Assert.Throws<ShiftTradeRequestStatusException>(() => _target.AddShiftTradeSwapDetail(new ShiftTradeSwapDetail(_requestedPerson,_requestedPerson,_shiftTradeSwapDetail.DateFrom,_shiftTradeSwapDetail.DateTo)));
        }

        [Test]
        public void VerifyCannotAddShiftTradeSwapDetailsWhenNotValid()
        {
            _target.SetShiftTradeStatus(ShiftTradeStatus.NotValid,_authorization);
			Assert.Throws<ShiftTradeRequestStatusException>(() => _target.AddShiftTradeSwapDetail(new ShiftTradeSwapDetail(_requestedPerson, _requestedPerson, _shiftTradeSwapDetail.DateFrom, _shiftTradeSwapDetail.DateTo)));
        }

        [Test]
        public void VerifyCannotClearShiftTradeSwapDetailsWhenOkByBothParts()
        {
            _target.Accept(_tradePerson,new EmptyShiftTradeRequestSetChecksum(),_authorization);
			Assert.Throws<ShiftTradeRequestStatusException>(() => _target.ClearShiftTradeSwapDetails());
        }

        [Test]
        public void VerifyCannotClearShiftTradeSwapDetailsWhenNotValid()
        {
            _target.SetShiftTradeStatus(ShiftTradeStatus.NotValid,_authorization);
			Assert.Throws<ShiftTradeRequestStatusException>(() => _target.ClearShiftTradeSwapDetails());
        }

        [Test]
        public void VerifyPushMessageIsCreatedForToPersonWhenCreatingAShiftTradeRequest()
        {
            Assert.IsTrue(messageWillBeSentToTradePerson());
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

        private bool messageWillBeSentToRequestPerson()
        {
            return _target.ReceiversForNotification.Contains(_requestedPerson) && _target.ShouldNotifyWithMessage;
        }

        private bool messageWillBeSentToTradePerson()
        {
            return _target.ReceiversForNotification.Contains(_tradePerson) && _target.ShouldNotifyWithMessage;
        }

        private bool messageWillOnlyBeSentToRequestPerson()
        {
            return _target.ReceiversForNotification.Count==1 && messageWillBeSentToRequestPerson();
        }

        private bool messageWillOnlyBeSentToTradePerson()
        {
            return _target.ReceiversForNotification.Count == 1 && messageWillBeSentToTradePerson();
        }

        private bool messageWillBeSentToBothPersons()
        {
            return messageWillBeSentToRequestPerson() && messageWillBeSentToTradePerson();
        }

		private static void setPersonLanguage(IPerson person, CultureInfo cultureInfo)
		{
			person.PermissionInformation.SetUICulture(cultureInfo);
		}

		private static void setPersonCulture(IPerson person, CultureInfo cultureInfo)
		{
			person.PermissionInformation.SetCulture(cultureInfo);
		}

        [Test]
        public void VerifySwappingShiftsForMoreThanOneDay()
        {
            var shiftTradeSwapDetail1 = new ShiftTradeSwapDetail(_requestedPerson, _tradePerson,
                                                             new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16));

            var shiftTradeSwapDetail2 = new ShiftTradeSwapDetail(_requestedPerson, _tradePerson,
                                                             new DateOnly(2008, 7, 17), new DateOnly(2008, 7, 17));

            _authorization = new PersonRequestAuthorizationCheckerForTest();
            var target = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail1, shiftTradeSwapDetail2 });
            
            MockRepository mocks = new MockRepository();
            IRequestApprovalService requestApprovalService =
                mocks.StrictMock<IRequestApprovalService>();
            PersonRequest personRequest = new PersonRequest(_requestedPerson, target);

            Expect.Call(requestApprovalService.Approve(null)).Return(new List<IBusinessRuleResponse>()).IgnoreArguments();

            mocks.ReplayAll();

			personRequest.Pending();
			IList<IBusinessRuleResponse> brokenRules = personRequest.Approve(requestApprovalService, _authorization);
			Assert.AreEqual(0, brokenRules.Count);

			Assert.That(target.TextForNotification, Is.Not.Null.Or.Empty);

			mocks.VerifyAll();
        }

		[Test]
		public void ShouldPendingMessageBeOnTheReceiverCultureLanguage()
		{
			setPersonCulture(_tradePerson, CultureInfoFactory.CreateSwedishCulture());
			setPersonLanguage(_tradePerson, CultureInfoFactory.CreateSwedishCulture());

			var shiftTradeSwapDetail1 = new ShiftTradeSwapDetail(_requestedPerson, _tradePerson,
															 new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16));

			var shiftTradeSwapDetail2 = new ShiftTradeSwapDetail(_requestedPerson, _tradePerson,
															 new DateOnly(2008, 7, 17), new DateOnly(2008, 7, 17));

			_authorization = new PersonRequestAuthorizationCheckerForTest();
			var target = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail1, shiftTradeSwapDetail2 });

			PersonRequest personRequest = new PersonRequest(_requestedPerson, target);
			personRequest.Pending();

			var notificationString = "Ny skiftbytesförfrågan 2008-07-16 - 2008-07-17, godkänn eller avslå i din lista.";

			Assert.AreEqual(notificationString, target.TextForNotification, "Pending message should be in the receiver's language");
		}

        [Test]
        public void VerifyCanAddAndClearShiftTradeSwapDetailsForMultipleDays()
        {
            DateOnly dateOnly = new DateOnly(2009, 9, 3);
            DateOnly dateOnly2 = new DateOnly(2009, 9, 4);
            Assert.AreNotEqual(new DateTimePeriod(),_target.Period);
            Assert.AreNotEqual(0,_target.ShiftTradeSwapDetails.Count);
            _target.ClearShiftTradeSwapDetails();
            Assert.AreEqual(0,_target.ShiftTradeSwapDetails.Count);
            Assert.AreEqual(new DateTimePeriod(), _target.Period);
            _target.AddShiftTradeSwapDetail(new ShiftTradeSwapDetail(_requestedPerson,_requestedPerson,dateOnly,dateOnly));
            _target.AddShiftTradeSwapDetail(new ShiftTradeSwapDetail(_requestedPerson,_requestedPerson,dateOnly2,dateOnly2));
            Assert.That(_target.TextForNotification, Is.Not.Null.Or.Empty);
            Assert.AreEqual(2,_target.ShiftTradeSwapDetails.Count);
        }

        [Test]
        public void VerifyDenyForMultipleDays()
        {
            var shiftTradeSwapDetail1 = new ShiftTradeSwapDetail(_requestedPerson, _tradePerson,
                                                             new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16));

            var shiftTradeSwapDetail2 = new ShiftTradeSwapDetail(_requestedPerson, _tradePerson,
                                                             new DateOnly(2008, 7, 17), new DateOnly(2008, 7, 17));

            _authorization = new PersonRequestAuthorizationCheckerForTest();
            var target = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail1, shiftTradeSwapDetail2 });
            target.Deny(_tradePerson);
            Assert.IsNotNull(target);

            //var datepattern = _requestedPerson.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern;
            //var notificationString = string.Format(UserTexts.Resources.ShiftTradeRequestForOneDayHasBeenDeniedDot,
            //                                       target.Period.StartDateTimeLocal(
            //                                           _requestedPerson.PermissionInformation.DefaultTimeZone()).
            //                                           ToString(datepattern));
            Assert.That(target.TextForNotification, Is.Not.Null.Or.Empty);
        }
    }
}
