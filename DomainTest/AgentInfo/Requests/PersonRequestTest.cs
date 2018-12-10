using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;


namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
	/// <summary>
	/// Tests for the PersonRequest class
	/// </summary>
	/// <remarks>
	/// Created by: robink
	/// Created date: 2008-06-05
	/// </remarks>
	[TestFixture]
	public class PersonRequestTest
	{
		private MockRepository _mocks;
		private IPerson _person;
		private readonly string _message = "I need some vacation!";
		private IPersonRequest _target;
		private TextRequest _textRequest;
		private readonly IPersonRequestCheckAuthorization _authorization = new PersonRequestAuthorizationCheckerForTest();

		/// <summary>
		/// Setups this instance.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-05
		/// </remarks>
		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_person = PersonFactory.CreatePerson();
			_textRequest = new TextRequest(new DateTimePeriod());
			_target = new PersonRequest(_person, _textRequest);
			_target.TrySetMessage(_message);
		}

		/// <summary>
		/// Verifies the empty constructor.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-05
		/// </remarks>
		[Test]
		public void VerifyEmptyConstructor()
		{
			Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType(), true));
		}

		/// <summary>
		/// Verifies the instance created.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-05
		/// </remarks>
		[Test]
		public void VerifyInstanceCreated()
		{
			Assert.IsNotNull(_target);
			Assert.AreEqual(_person, _target.Person);
			Assert.AreEqual(_message, _target.GetMessage(new NoFormatting()));
		}

		/// <summary>
		/// Verifies the properties.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-05
		/// </remarks>
		[Test]
		public void VerifyProperties()
		{
			const string newMessage = "I need parental leave.";
			_target.TrySetMessage(newMessage);
			_target.Changed = true;

			Assert.AreEqual(newMessage, _target.GetMessage(new NoFormatting()));
			Assert.IsTrue(_target.IsNew);
			Assert.IsTrue(_target.Changed);
		}

		[Test]
		public void ShouldNotSendPushMessageWithSameMessage()
		{
			_textRequest.TextForNotification = string.Empty;
			_target.TrySetMessage(_message);

			Assert.AreEqual(string.Empty, _textRequest.TextForNotification);
		}

		/// <summary>
		/// check that answer from adminsitrator is handled correctly
		/// </summary>
		[Test]
		public void AddAnswer()
		{
			const string newMessage = "I need parental leave.";
			_target.TrySetMessage(newMessage);

			const string answerMessage = "Yes, go ahead";
			_target.Reply(answerMessage);

			Assert.AreEqual(newMessage + Environment.NewLine + answerMessage, _target.GetMessage(new NoFormatting()));
		}
		[Test]
		public void VerifyChangedIsSetCorrect()
		{
			_target.Deny(null, _authorization);
			Assert.IsTrue(_target.Changed);
			_target.Persisted();
			Assert.IsFalse(_target.Changed);
		}

		[Test]
		public void SetSubject()
		{
			const string subject = "Parental leave.";
			const string newMessage = "I need parental leave.";
			_target.Subject = subject;
			_target.TrySetMessage(newMessage);

			Assert.AreEqual(subject, _target.GetSubject(new NoFormatting()));
		}


		[Test]
		public void SetNullAsMessage()
		{
			_target.TrySetMessage(null);
			_target.GetMessage(new NoFormatting()).Should().Be.Empty();
		}

		/// <summary>
		/// Verifies the message cannot be changed when denied.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-09
		/// </remarks>
		[Test]
		public void VerifyMessageCannotBeChangedWhenDenied()
		{
			_target.Deny(null, _authorization);
			_target.Persisted();

			Assert.Throws<InvalidOperationException>(() => _target.TrySetMessage(_message));
		}

		/// <summary>
		/// Verifies the message cannot be changed when approved.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-09
		/// </remarks>
		[Test]
		[Ignore("approved absence request now can be cancelled, maybe allowed to reply")]
		public void VerifyMessageCannotBeChangedWhenApproved()
		{
			_target.Pending();
			_target.Approve(null, _authorization);
			_target.Persisted();
			Assert.Throws<InvalidOperationException>(() => _target.TrySetMessage(_message));
		}

		[Test]
		public void VerifyDeny()
		{
			_target.Pending();
			Assert.IsFalse(_target.IsNew);

			_target.Deny( "DeniedDueToRain", _authorization);
			Assert.IsTrue(_target.IsDenied);
			Assert.IsFalse(_target.IsAutoDenied);

			Assert.AreEqual("DeniedDueToRain", _target.DenyReason);

			_target.ForcePending();
			_target.Deny(null, _authorization);
			Assert.AreEqual(string.Empty, _target.DenyReason);
		}

		[Test]
		public void VerifyAutoDeny()
		{
			Assert.IsTrue(_target.IsNew);

			_target.Deny( "DeniedDueToRain", _authorization, null, PersonRequestDenyOption.AutoDeny);
			Assert.IsTrue(_target.IsDenied);
			Assert.IsTrue(_target.IsAutoDenied);

			Assert.AreEqual("DeniedDueToRain", _target.DenyReason);
		}

		/// <summary>
		/// Verifies the approve.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-09
		/// </remarks>
		[Test]
		public void VerifyApprove()
		{
			_target.Pending();
			IList<IBusinessRuleResponse> brokenRules = _target.Approve(null, _authorization);
			Assert.AreEqual(0, brokenRules.Count);
			Assert.IsTrue(_target.IsApproved);
			Assert.IsFalse(_target.IsAutoAproved);
		}

		[Test]
		public void VerifyApprove_AutoApproved()
		{
			_target.Pending();
			var brokenRules = _target.Approve(null, _authorization, true);
			brokenRules.Count.Should().Be.EqualTo(0);
			_target.IsApproved.Should().Be.True();
			_target.IsAutoAproved.Should().Be.True();
		}

		[Test]
		public void VerifyPending()
		{
			_mocks.ReplayAll();
			_target.Pending();
			Assert.IsTrue(_target.IsPending);
			_target.Approve(null, _authorization);
			Assert.IsTrue(_target.IsApproved);
			_mocks.VerifyAll();
		}


		[Test]
		public void VerifyPendingDoesNothingWhenNotAllowed()
		{
			IPerson person = PersonFactory.CreatePerson("Kalle", "kula");
			IPersonRequest personRequest = new PersonRequest(person);
			personRequest.Deny(null, _authorization);

			Assert.IsTrue(personRequest.IsDenied);
			personRequest.Pending();
			Assert.IsTrue(personRequest.IsDenied);

			// Force pending...
			personRequest.ForcePending();
			Assert.IsTrue(personRequest.IsPending);
			Assert.IsNotNull(_target); // To avoid FxCop

		}

		/// <summary>
		/// Verifies the approve with request parts.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-09
		/// </remarks>
		[Test]
		public void VerifyApproveWithRequest()
		{
			Request request1 = _mocks.StrictMock<Request>();

			((IRequest)request1).SetParent(_target);
			LastCall.Repeat.Once();
			Expect.Call(request1.GetHashCode()).Return(37);

			typeof(Request).GetMethod("Approve", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(request1, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, new object[] { null }, CultureInfo.InvariantCulture);
			LastCall.Repeat.Once().Return(new List<IBusinessRuleResponse>());

			_mocks.ReplayAll();

			_target.Request = request1;
			_target.Pending();
			IList<IBusinessRuleResponse> brokenRules = _target.Approve(null, _authorization);
			Assert.AreEqual(0, brokenRules.Count);

			_mocks.VerifyAll();
		}

		/// <summary>
		/// Verifies the deny with request parts.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-09
		/// </remarks>
		[Test]
		public void VerifyDenyWithRequest()
		{

			IRequest request1 = _mocks.StrictMock<IRequest>();

			request1.SetParent(_target);
			LastCall.Repeat.Once();

			request1.Deny(null);
			LastCall.Repeat.Once();

			_mocks.ReplayAll();

			_target.Request = request1;
			_target.Deny( null, _authorization);

			_mocks.VerifyAll();
		}

		[Test]
		public void CanMoveToApprovedFromDeniedWhenWaitlistingIsEnabled()
		{
			var personRequest = tryMovePersonRequestFromDeniedToApproved(true);
			Assert.IsTrue(personRequest.IsApproved);
		}

		[Test]
		public void CannotMoveToApprovedFromDeniedWhenWaitlistingIsDisabled()
		{
			try
			{
				tryMovePersonRequestFromDeniedToApproved(false);
			}
			catch (Exception ex)
			{

				Assert.IsTrue(ex is InvalidRequestStateTransitionException);
			}
			
		}
		
		/// <summary>
		/// Verifies the person cannot be null.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-05
		/// </remarks>
		[Test]
		public void VerifyPersonCannotBeNull()
		{
			Assert.Throws<ArgumentNullException>(() => _target = new PersonRequest(null));
		}

		/// <summary>
		/// Verifies the maximum length of message.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-05
		/// </remarks>
		[Test]
		public void VerifyMaximumLengthOfMessage()
		{
			Assert.IsFalse(_target.TrySetMessage(_message.PadLeft(2001)));
		}

		/// <summary>
		/// Verifies the date without requests.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-09
		/// </remarks>
		[Test]
		public void VerifyDateWithoutRequests()
		{
			_target = new PersonRequest(_person);
			Assert.AreEqual(DateTime.Today, _target.RequestedDate);

			ReflectionHelper.SetCreatedOn(_target, DateTime.Today.AddDays(-3));

			Assert.AreEqual(DateTime.Today.AddDays(-3), _target.RequestedDate);
		}

		/// <summary>
		/// Verifies the date with requests.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-09
		/// </remarks>
		[Test]
		public void VerifyDateWithRequest()
		{

			IRequest request1 = _mocks.StrictMock<IRequest>();

			request1.SetParent(_target);
			LastCall.Repeat.Once();

			DateTimePeriod period1 = new DateTimePeriod(new DateTime(2008, 7, 16, 0, 0, 0, DateTimeKind.Utc),
														new DateTime(2008, 7, 19, 0, 0, 0, DateTimeKind.Utc));
			Expect.Call(request1.Period).Return(period1).Repeat.AtLeastOnce();

			_mocks.ReplayAll();

			_target.Request = request1;

			Assert.AreEqual(period1.StartDateTime, _target.RequestedDate);

			_mocks.VerifyAll();
		}

		[Test]
		public void VerifyClone()
		{
			IAbsenceRequest part = new AbsenceRequest(AbsenceFactory.CreateAbsence("abs"), new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
			_target.Request = part;
			_target.SetId(Guid.NewGuid());
			part.SetId(Guid.NewGuid());
			IPersonRequest entityClone = _target.EntityClone();

			Assert.AreEqual(_target.Person, entityClone.Person);
			Assert.AreEqual(_target.Id, entityClone.Id);
			Assert.AreEqual(_target.Request.Id, entityClone.Request.Id);
			Assert.AreEqual(_target.Request, entityClone.Request);
			Assert.AreNotSame(_target.Request, entityClone.Request);
			Assert.AreEqual(_target.GetMessage(new NoFormatting()), entityClone.GetMessage(new NoFormatting()));
			Assert.AreEqual(_target.CreatedOn, entityClone.CreatedOn);
			Assert.AreEqual(((PersonRequest)_target).IsDeleted, ((PersonRequest)entityClone).IsDeleted);
			Assert.AreEqual(_target.RequestedDate, entityClone.RequestedDate);
			Assert.AreEqual(_target.IsNew, entityClone.IsNew);

			entityClone = _target.NoneEntityClone();

			Assert.AreEqual(_target.Person, entityClone.Person);
			Assert.IsNull(entityClone.Id);
			Assert.IsNull(entityClone.Request.Id);
			Assert.AreEqual(_target.GetMessage(new NoFormatting()), entityClone.GetMessage(new NoFormatting()));
			Assert.AreEqual(_target.CreatedOn, entityClone.CreatedOn);
			Assert.AreEqual(((PersonRequest)_target).IsDeleted, ((PersonRequest)entityClone).IsDeleted);
			Assert.AreNotEqual(_target.Request, entityClone.Request);
			Assert.AreNotSame(_target.Request, entityClone.Request);
			Assert.AreEqual(_target.RequestedDate, entityClone.RequestedDate);
			Assert.AreEqual(_target.IsNew, entityClone.IsNew);

			PersonRequest clone = (PersonRequest)_target.Clone();
			Assert.AreNotEqual(_target, clone);
		}

		[Test]
		public void TestPropertyNotification()
		{
			bool notificationReceived = false;
			_target.PropertyChanged += (sender, e) => { notificationReceived = true; };
			_target.TrySetMessage("Note this");
			Assert.IsTrue(notificationReceived);
		}

		[Test]
		public void VerifyIsEditable()
		{
			Assert.IsTrue(_target.IsEditable);
			_target.Deny(null, _authorization);
			_target.Persisted();
			Assert.IsFalse(_target.IsEditable);
		}

		[Test]
		public void VerifyChecksRequestToNotify()
		{

			IRequest request = _mocks.StrictMock<IRequest>();

			using (_mocks.Record())
			{
				Expect.Call(() => request.SetParent(_target)).IgnoreArguments();
				Expect.Call(request.ShouldNotifyWithMessage).Return(true);
			}
			using (_mocks.Playback())
			{
				_target = new PersonRequest(_person);
				_target.Request = request;
				Assert.IsTrue(((PersonRequest)_target).ShouldSendPushMessageWhenAltered());
			}
		}

		[Test]
		public void VerifyCreatesPushMessageFromRequest()
		{
			string pushMessageText = "text";


			IRequest request = _mocks.StrictMock<IRequest>();

			using (_mocks.Record())
			{
				Expect.Call(() => request.SetParent(_target)).IgnoreArguments();
				Expect.Call(request.ReceiversForNotification).Return(new List<IPerson> { _person });
				Expect.Call(request.TextForNotification).Return(pushMessageText).Repeat.Any();
			}
			using (_mocks.Playback())
			{
				_target = new PersonRequest(_person);
				_target.Subject = "some subject that will be set as title";
				_target.Request = request;
				_target.Pending();
				ISendPushMessageService sendPushMessageService = ((PersonRequest)_target).PushMessageWhenAlteredInformation();
				Assert.IsTrue(sendPushMessageService.Receivers.Contains(_person));
				Assert.AreEqual(sendPushMessageService.PushMessage.GetMessage(new NoFormatting()), pushMessageText);
				Assert.AreEqual(sendPushMessageService.PushMessage.MessageType, MessageType.Information);
				Assert.AreEqual(sendPushMessageService.PushMessage.GetTitle(new NoFormatting()), _target.GetSubject(new NoFormatting()));
				Assert.IsTrue(sendPushMessageService.PushMessage.TranslateMessage, "Verify that the message will be translated (since its autogenerated)");
				Assert.IsTrue(sendPushMessageService.PushMessage.ReplyOptions.Contains("OK"));
			}
		}

		[Test]
		public void ShouldSetShiftTradeFromOfferType()
		{
			var currentShift = ScheduleDayFactory.Create(DateOnly.Today.AddDays(1));
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(_person, PersonFactory.CreatePerson("Receiver"), DateOnly.Today, DateOnly.Today);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
			shiftTradeRequest.Offer = new ShiftExchangeOffer(currentShift, new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Pending);
			IRequest request = shiftTradeRequest;

			using (_mocks.Playback())
			{
				_target = new PersonRequest(_person);
				_target.Subject = "some subject that will be set as title";
				_target.Request = request;
				_target.Pending();
				ISendPushMessageService sendPushMessageService = ((PersonRequest)_target).PushMessageWhenAlteredInformation();

				Assert.AreEqual(sendPushMessageService.PushMessage.MessageType, MessageType.ShiftTradeFromOffer);
			}
		}

		[Test]
		public void ShouldTruncateMessageWhenSendingPushMessage()
		{
			string pushMessageText = "a".PadRight(500, 'a');

			_target.TrySetMessage(pushMessageText);
			Assert.AreEqual(_target.GetMessage(new NoFormatting()), pushMessageText);
			Assert.AreEqual(255, _target.Request.TextForNotification.Length);
		}

		[Test]
		public void VerifyCreatesSetsTitleToTextForNotificationIfNoSubjectOnRequest()
		{
			string pushMessageText = "text";


			IRequest request = _mocks.StrictMock<IRequest>();

			using (_mocks.Record())
			{
				Expect.Call(() => request.SetParent(_target)).IgnoreArguments();
				Expect.Call(request.ReceiversForNotification).Return(new List<IPerson> { _person });
				Expect.Call(request.TextForNotification).Return(pushMessageText).Repeat.Any();
			}
			using (_mocks.Playback())
			{
				_target = new PersonRequest(_person);
				_target.Subject = String.Empty;
				_target.Request = request;
				_target.Pending();
				ISendPushMessageService sendPushMessageService = ((PersonRequest)_target).PushMessageWhenAlteredInformation();
				Assert.IsTrue(sendPushMessageService.Receivers.Contains(_person));
				Assert.AreEqual(sendPushMessageService.PushMessage.GetMessage(new NoFormatting()), pushMessageText);
				Assert.AreEqual(sendPushMessageService.PushMessage.GetTitle(new NoFormatting()), pushMessageText);
			}
		}

		[Test]
		public void VerifyNoMessageIsSentWhenNew()
		{
			IRequest request = _mocks.StrictMock<IRequest>();

			using (_mocks.Record())
			{
				Expect.Call(() => request.SetParent(_target)).IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				_target = new PersonRequest(_person);
				_target.Subject = String.Empty;
				_target.Request = request;
				Assert.IsNull(((PersonRequest)_target).PushMessageWhenAlteredInformation());
			}
		}

		[Test]
		public void CheckMessageLength()
		{
			_target = new PersonRequest(_person);
			var mess = new StringBuilder("A");

			Assert.IsTrue(_target.CheckReplyTextLength(mess.ToString()));
			mess.Append("A".PadRight(2001, 'A'));

			Assert.IsFalse(_target.CheckReplyTextLength(mess.ToString()));

		}

		[Test]
		public void GetMessageThrowExceptionIfThereIsNoTextFormatter()
		{
			Assert.Throws<ArgumentNullException>(() => _target.GetMessage(null));
		}

		[Test]
		public void GetSubjectThrowExceptionIfThereIsNoTextFormatter()
		{
			Assert.Throws<ArgumentNullException>(() => _target.GetSubject(null));
		}

		[Test]
		public void VerifyCreateMemento()
		{
			Assert.IsNotNull(_target.CreateMemento());
		}

		[Test]
		public void VerifyRestore()
		{
			var previousState = new PersonRequest(_person);
			_target.Restore(previousState);
		}

		[Test]
		public void ReturnTrueIfMessageIsEmpty()
		{
			Assert.IsTrue(_target.Reply(null));
		}

		[Test]
		public void VerifyUnderlyingStateId()
		{
			var personRequest = new PersonRequest(_person);
			var result = PersonRequest.GetUnderlyingStateId(personRequest);
			Assert.AreEqual(result, 3);
		}

		[Test]
		public void ShouldReturnUpdatedOnServerUtc()
		{
			_target.UpdatedOnServerUtc.Should().Be.EqualTo(new DateTime());
		}

		[Test]
		public void ShouldCallDummyToRemoveUnusedMethod()
		{
			((PersonRequest)_target).DummyMethodToRemoveCompileErrorsWithUnusedVariable();
		}


		[Test]
		public void SendChangeOverMessageBroker_NewRequest_ReturnFalse()
		{
			_target.SendChangeOverMessageBroker().Should().Be.EqualTo(false);
		}

		[Test]
		public void SendChangeOverMessageBroker_NewToPending_ReturnTrue()
		{
			_target.Persisted();
			_target.Pending();
			_target.SendChangeOverMessageBroker().Should().Be.EqualTo(true);
		}

		[Test]
		public void SendChangeOverMessageBroker_PendingToApproved_ReturnTrue()
		{
			_target.Pending();
			_target.Persisted();
			_target.Approve(null, _authorization);
			_target.SendChangeOverMessageBroker().Should().Be.EqualTo(true);
		}

		[Test]
		public void SendChangeOverMessageBroker_PendingToDenied_ReturnTrue()
		{
			_target.Pending();
			_target.Persisted();
			_target.Deny( null, _authorization);
			_target.SendChangeOverMessageBroker().Should().Be.EqualTo(true);
		}

		[Test]
		public void SendChangeOverMessageBroker_NewToAutoDenied_ReturnTrue()
		{
			_target.Deny(null, _authorization);
			_target.SendChangeOverMessageBroker().Should().Be.EqualTo(true);
		}

		[Test]
		public void SendChangeOverMessageBroker_SimulateDeleteFromMyTimeWeb_ReturnTrue()
		{
			_target.SendChangeOverMessageBroker().Should().Be.EqualTo(false);
		}

		private void setupShiftTrade()
		{
			_target.Request = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(new Person(), new Person(), new DateOnly(2013,08,26), new DateOnly(2013,08,27))
				});
		}

		[Test]
		public void ShiftTrade_SendChangeOverMessageBroker_SentToOtherPerson_ReturnFalse()
		{
			setupShiftTrade();
			_target.Pending();
			_target.SendChangeOverMessageBroker().Should().Be.EqualTo(false);
		}

		[Test]
		public void ShiftTrade_SendChangeOverMessageBroker_NotPickedUpByBus_ReturnFalse()
		{
			setupShiftTrade();
			_target.SendChangeOverMessageBroker().Should().Be.EqualTo(false);
		}

		[Test]
		public void ShiftTrade_SendChangeOverMessageBroker_AcceptedByOther_ReturnTrue()
		{
			setupShiftTrade();
			_target.Pending();
			_target.Persisted();
			_target.SendChangeOverMessageBroker().Should().Be.EqualTo(true);
		}

		[Test]
		public void ShiftTrade_SendChangeOverMessageBroker_DeniedByOther_ReturnFalse()
		{
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(new Person(), new Person(), new DateOnly(2013,08,26), new DateOnly(2013,08,27))
				});
			shiftTradeRequest.SetShiftTradeStatus(ShiftTradeStatus.OkByMe, _authorization);
			_target.Request = shiftTradeRequest;
			_target.Pending();
			_target.Persisted();
			_target.Deny(null, _authorization);
			_target.SendChangeOverMessageBroker().Should().Be.EqualTo(false);
		}


		[Test]
		public void ShiftTrade_SendChangeOverMessageBroker_DeniedByOtherAfterAproval_ReturnTrue()
		{
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(new Person(), new Person(), new DateOnly(2013,08,26), new DateOnly(2013,08,27))
				});
			shiftTradeRequest.SetShiftTradeStatus(ShiftTradeStatus.OkByBothParts, _authorization);
			_target.Request = shiftTradeRequest;
			_target.Pending();
			_target.Deny( null, _authorization);
			_target.Persisted();
			_target.SendChangeOverMessageBroker().Should().Be.EqualTo(true);
		}

		[Test]
		public void ShiftTrade_SendChangeOverMessageBroker_DeniedByAdmin_ReturnTrue()
		{
			setupShiftTrade();
			_target.Pending();
			_target.Deny(null, _authorization);
			_target.Persisted();
			_target.SendChangeOverMessageBroker().Should().Be.EqualTo(true);
		}

		[Test]
		public void ShiftTrade_SendChangeOverMessageBroker_AutoApproved_ReturnFalse()
		{
			var approvalService = new ApprovalServiceForTest();

			setupShiftTrade();
			_target.Pending();
			_target.Persisted();
			_target.Approve(approvalService, _authorization, true);
			_target.SendChangeOverMessageBroker().Should().Be.EqualTo(false);
		}

		[Test]
		public void ShiftTrade_SendChangeOverMessageBroker_NewPendingOkByBothParts_ReturnTrue()
		{
			setupShiftTrade();
			_target.Persisted();
			_target.Pending();
			((ShiftTradeRequest)_target.Request).SetShiftTradeStatus(ShiftTradeStatus.OkByBothParts, _authorization);
			_target.SendChangeOverMessageBroker().Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldDenyWaitlistedRequestWithAlreadyAbsence()
		{
			var person = PersonFactory.CreatePerson("Kalle", "kula");
			var waitlistedPersonRequest = new PersonRequest(person);
			var absence = new Absence();
			person.WorkflowControlSet = createWorkFlowControlSet(absence, new GrantAbsenceRequest(), true);
			waitlistedPersonRequest.Request = new AbsenceRequest(absence, new DateTimePeriod(2016, 9, 6, 2016, 9, 6));
			waitlistedPersonRequest.ForcePending();
			waitlistedPersonRequest.Deny(null, _authorization, null, PersonRequestDenyOption.AutoDeny);
			waitlistedPersonRequest.IsWaitlisted.Should().Be(true);

			waitlistedPersonRequest.Deny(null, _authorization, null,
				PersonRequestDenyOption.AutoDeny | PersonRequestDenyOption.AlreadyAbsence);
			waitlistedPersonRequest.IsDenied.Should().Be(true);
			waitlistedPersonRequest.IsWaitlisted.Should().Be(false);
		}

		[Test]
		public void ShouldDenyExpiredRequestWithWaitlistEnabled()
		{
			var person = PersonFactory.CreatePerson("Kalle", "kula");
			var waitlistedPersonRequest = new PersonRequest(person);
			var absence = new Absence();
			person.WorkflowControlSet = createWorkFlowControlSet(absence, new GrantAbsenceRequest(), true);
			waitlistedPersonRequest.Request = new AbsenceRequest(absence, new DateTimePeriod(2016, 9, 6, 2016, 9, 6));
			waitlistedPersonRequest.ForcePending();
			waitlistedPersonRequest.Deny(null, _authorization, null,PersonRequestDenyOption.AutoDeny | PersonRequestDenyOption.RequestExpired);
			waitlistedPersonRequest.IsDenied.Should().Be(true);
			waitlistedPersonRequest.IsWaitlisted.Should().Be(false);
		}

		[Test]
		public void ShouldDenyWaitlistedRequestWithExpiredStatus()
		{
			var person = PersonFactory.CreatePerson("Kalle", "kula");
			var waitlistedPersonRequest = new PersonRequest(person);
			var absence = new Absence();
			person.WorkflowControlSet = createWorkFlowControlSet(absence, new GrantAbsenceRequest(), true);
			waitlistedPersonRequest.Request = new AbsenceRequest(absence, new DateTimePeriod(2016, 9, 6, 2016, 9, 6));
			waitlistedPersonRequest.ForcePending();
			waitlistedPersonRequest.Deny(null, _authorization, null, PersonRequestDenyOption.AutoDeny);
			waitlistedPersonRequest.IsWaitlisted.Should().Be(true);

			waitlistedPersonRequest.Deny( null, _authorization, null, PersonRequestDenyOption.AutoDeny | PersonRequestDenyOption.RequestExpired);
			waitlistedPersonRequest.IsDenied.Should().Be(true);
			waitlistedPersonRequest.IsWaitlisted.Should().Be(false);
		}

		private PersonRequest tryMovePersonRequestFromDeniedToApproved(bool waitlistingEnabled)
		{
			var absence = new Absence();
			var startDateTime = new DateTime(2016, 01, 01, 00, 00, 00, DateTimeKind.Utc);
			var endDateTime = new DateTime(2016, 01, 01, 23, 59, 00, DateTimeKind.Utc);

			var factory = new PersonRequestFactory();

			var absenceRequest = factory.CreateAbsenceRequest(absence, new DateTimePeriod(startDateTime, endDateTime));
			var personRequest = absenceRequest.Parent as PersonRequest;
			personRequest.SetNew();
			var person = absenceRequest.Person;

			person.WorkflowControlSet = createWorkFlowControlSet(absence, new GrantAbsenceRequest(), waitlistingEnabled);

			personRequest.Deny( null, _authorization);

			personRequest.Approve(new ApprovalServiceForTest(), _authorization, true);
			return personRequest;
		}


		private static WorkflowControlSet createWorkFlowControlSet(IAbsence absence, IProcessAbsenceRequest processAbsenceRequest, bool waitlistingIsEnabled)
		{
			var workflowControlSet = new WorkflowControlSet { AbsenceRequestWaitlistEnabled = waitlistingIsEnabled };
			var startDate = new DateTime(2015, 12, 12, 00, 00, 00, DateTimeKind.Utc);
			var endDate = new DateTime(DateTime.Now.Year + 1, 12, 12, 00, 00, 00, DateTimeKind.Utc);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				AbsenceRequestProcess = processAbsenceRequest,
				Period = dateOnlyPeriod,
				OpenForRequestsPeriod = dateOnlyPeriod
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			return workflowControlSet;

		}

	}
}
