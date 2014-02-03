using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.SignalR;

namespace Teleopti.MessagingTest.SignalR
{
	[TestFixture]
	public class SignalSenderTest
	{
		private Task _doneTask;

		[SetUp]
		public void Setup()
		{
			var taskCompletionSource = new TaskCompletionSource<object>();
			taskCompletionSource.SetResult(null);
			_doneTask = taskCompletionSource.Task;
		}

		[Test]
		public void ShouldSendRtaNotification()
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Return(_doneTask);
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalSenderForTest(hubConnection, DateTime.UtcNow);
			target.InstantiateBrokerService();
			target.QueueRtaNotification(Guid.Empty, Guid.Empty, new ActualAgentState());
			target.WaitUntilQueueProcessed();

			hubProxy.AssertWasCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("NotifyClientsMultiple"), 
				Arg<IEnumerable<Notification>>.List.Count(Rhino.Mocks.Constraints.Is.Equal(1))));
		}

		[Test]
		public void ShouldRetryFailedNotification()
		{
			var taskCompletionSource = new TaskCompletionSource<object>();
			taskCompletionSource.SetException(new InvalidOperationException());
			var failedTask = taskCompletionSource.Task;

			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Repeat.Once().Return(failedTask);
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Repeat.Once().Return(_doneTask);
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalSenderForTest(hubConnection, DateTime.UtcNow);
			target.InstantiateBrokerService();
			target.QueueRtaNotification(Guid.Empty, Guid.Empty, new ActualAgentState());
			target.WaitUntilQueueProcessed();

			hubProxy.AssertWasCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("NotifyClientsMultiple"),
				Arg<IEnumerable<Notification>>.List.Count(Rhino.Mocks.Constraints.Is.Equal(1))), a => a.Repeat.Twice());
		}

		[Test]
		public void ShouldIgnoreAfterThreeRetries()
		{
			var taskCompletionSource = new TaskCompletionSource<object>();
			taskCompletionSource.SetException(new InvalidOperationException());
			var failedTask = taskCompletionSource.Task;

			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Repeat.Times(3).Return(failedTask);
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalSenderForTest(hubConnection, DateTime.UtcNow);
			target.InstantiateBrokerService();
			target.QueueRtaNotification(Guid.Empty, Guid.Empty, new ActualAgentState());
			target.WaitUntilQueueProcessed();

			hubProxy.AssertWasCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("NotifyClientsMultiple"),
				Arg<IEnumerable<Notification>>.List.Count(Rhino.Mocks.Constraints.Is.Equal(1))), a => a.Repeat.Times(3));
		}

		[Test]
		public void ShouldBatchTwentyNotificationsAtATime()
		{
			Assert.Ignore("Test describing existing functionality, implement in future feature");
		}

		[Test]
		public void ShouldDiscardNotificationsOlderThanTwoMinutes()
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Expect(x => x.Invoke("", null)).IgnoreArguments().Repeat.Times(3).Return(_doneTask);
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalSenderForTest(hubConnection, DateTime.UtcNow.AddMinutes(-2));
			target.InstantiateBrokerService();
			target.QueueRtaNotification(Guid.Empty, Guid.Empty, new ActualAgentState());
			target.WaitUntilQueueProcessed();
			
			hubProxy.AssertWasNotCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("NotifyClientsMultiple"), 
				Arg<IEnumerable<Notification>>.Is.Anything));
		}

		[Test]
		// ErikS: 2014-02-03
		// Unstable, not sure how to fix...
		//[TestCase(1)]
		//[TestCase(2)]
		//[TestCase(3)]
		//[TestCase(4)]
		//[TestCase(5)]
		//[TestCase(6)]
		//[TestCase(7)]
		//[TestCase(8)]
		//[TestCase(9)]
		//[TestCase(10)]
		//[TestCase(11)]
		//[TestCase(12)]
		//[TestCase(13)]
		//[TestCase(14)]
		//[TestCase(15)]
		//[TestCase(16)]
		//[TestCase(17)]
		//[TestCase(18)]
		//[TestCase(19)]
		//[TestCase(20)]
		//[TestCase(21)]
		//[TestCase(22)]
		//[TestCase(23)]
		//[TestCase(24)]
		//[TestCase(25)]
		//[TestCase(26)]
		//[TestCase(27)]
		//[TestCase(28)]
		//[TestCase(29)]
		//[TestCase(30)]
		//[TestCase(31)]
		//[TestCase(32)]
		//[TestCase(33)]
		//[TestCase(34)]
		//[TestCase(35)]
		//[TestCase(36)]
		//[TestCase(37)]
		//[TestCase(38)]
		//[TestCase(39)]
		//[TestCase(40)]
		//[TestCase(41)]
		//[TestCase(42)]
		//[TestCase(43)]
		//[TestCase(44)]
		//[TestCase(45)]
		//[TestCase(46)]
		//[TestCase(47)]
		//[TestCase(48)]
		//[TestCase(49)]
		//[TestCase(50)]
		//[TestCase(51)]
		//[TestCase(52)]
		//[TestCase(53)]
		//[TestCase(54)]
		//[TestCase(55)]
		//[TestCase(56)]
		//[TestCase(57)]
		//[TestCase(58)]
		//[TestCase(59)]
		//[TestCase(60)]
		//[TestCase(61)]
		//[TestCase(62)]
		//[TestCase(63)]
		//[TestCase(64)]
		//[TestCase(65)]
		//[TestCase(66)]
		//[TestCase(67)]
		//[TestCase(68)]
		//[TestCase(69)]
		//[TestCase(70)]
		//[TestCase(71)]
		//[TestCase(72)]
		//[TestCase(73)]
		//[TestCase(74)]
		//[TestCase(75)]
		//[TestCase(76)]
		//[TestCase(77)]
		//[TestCase(78)]
		//[TestCase(79)]
		//[TestCase(80)]
		//[TestCase(81)]
		//[TestCase(82)]
		//[TestCase(83)]
		//[TestCase(84)]
		//[TestCase(85)]
		//[TestCase(86)]
		//[TestCase(87)]
		//[TestCase(88)]
		//[TestCase(89)]
		//[TestCase(90)]
		//[TestCase(91)]
		//[TestCase(92)]
		//[TestCase(93)]
		//[TestCase(94)]
		//[TestCase(95)]
		//[TestCase(96)]
		//[TestCase(97)]
		//[TestCase(98)]
		//[TestCase(99)]
		//[TestCase(100)]
		//[TestCase(101)]
		//[TestCase(102)]
		//[TestCase(103)]
		//[TestCase(104)]
		//[TestCase(105)]
		//[TestCase(106)]
		//[TestCase(107)]
		//[TestCase(108)]
		//[TestCase(109)]
		//[TestCase(110)]
		//[TestCase(111)]
		//[TestCase(112)]
		//[TestCase(113)]
		//[TestCase(114)]
		//[TestCase(115)]
		//[TestCase(116)]
		//[TestCase(117)]
		//[TestCase(118)]
		//[TestCase(119)]
		//[TestCase(120)]
		//[TestCase(121)]
		//[TestCase(122)]
		//[TestCase(123)]
		//[TestCase(124)]
		//[TestCase(125)]
		//[TestCase(126)]
		//[TestCase(127)]
		//[TestCase(128)]
		//[TestCase(129)]
		//[TestCase(130)]
		//[TestCase(131)]
		//[TestCase(132)]
		//[TestCase(133)]
		//[TestCase(134)]
		//[TestCase(135)]
		//[TestCase(136)]
		//[TestCase(137)]
		//[TestCase(138)]
		//[TestCase(139)]
		//[TestCase(140)]
		//[TestCase(141)]
		//[TestCase(142)]
		//[TestCase(143)]
		//[TestCase(144)]
		//[TestCase(145)]
		//[TestCase(146)]
		//[TestCase(147)]
		//[TestCase(148)]
		//[TestCase(149)]
		//[TestCase(150)]
		//[TestCase(151)]
		//[TestCase(152)]
		//[TestCase(153)]
		//[TestCase(154)]
		//[TestCase(155)]
		//[TestCase(156)]
		//[TestCase(157)]
		//[TestCase(158)]
		//[TestCase(159)]
		//[TestCase(160)]
		//[TestCase(161)]
		//[TestCase(162)]
		//[TestCase(163)]
		//[TestCase(164)]
		//[TestCase(165)]
		//[TestCase(166)]
		//[TestCase(167)]
		//[TestCase(168)]
		//[TestCase(169)]
		//[TestCase(170)]
		//[TestCase(171)]
		//[TestCase(172)]
		//[TestCase(173)]
		//[TestCase(174)]
		//[TestCase(175)]
		//[TestCase(176)]
		//[TestCase(177)]
		//[TestCase(178)]
		//[TestCase(179)]
		//[TestCase(180)]
		//[TestCase(181)]
		//[TestCase(182)]
		//[TestCase(183)]
		//[TestCase(184)]
		//[TestCase(185)]
		//[TestCase(186)]
		//[TestCase(187)]
		//[TestCase(188)]
		//[TestCase(189)]
		//[TestCase(190)]
		//[TestCase(191)]
		//[TestCase(192)]
		//[TestCase(193)]
		//[TestCase(194)]
		//[TestCase(195)]
		//[TestCase(196)]
		//[TestCase(197)]
		//[TestCase(198)]
		//[TestCase(199)]
		//[TestCase(200)]
		//[TestCase(201)]
		//[TestCase(202)]
		//[TestCase(203)]
		//[TestCase(204)]
		//[TestCase(205)]
		//[TestCase(206)]
		//[TestCase(207)]
		//[TestCase(208)]
		//[TestCase(209)]
		//[TestCase(210)]
		//[TestCase(211)]
		//[TestCase(212)]
		//[TestCase(213)]
		//[TestCase(214)]
		//[TestCase(215)]
		//[TestCase(216)]
		//[TestCase(217)]
		//[TestCase(218)]
		//[TestCase(219)]
		//[TestCase(220)]
		//[TestCase(221)]
		//[TestCase(222)]
		//[TestCase(223)]
		//[TestCase(224)]
		//[TestCase(225)]
		//[TestCase(226)]
		//[TestCase(227)]
		//[TestCase(228)]
		//[TestCase(229)]
		//[TestCase(230)]
		//[TestCase(231)]
		//[TestCase(232)]
		//[TestCase(233)]
		//[TestCase(234)]
		//[TestCase(235)]
		//[TestCase(236)]
		//[TestCase(237)]
		//[TestCase(238)]
		//[TestCase(239)]
		//[TestCase(240)]
		//[TestCase(241)]
		//[TestCase(242)]
		//[TestCase(243)]
		//[TestCase(244)]
		//[TestCase(245)]
		//[TestCase(246)]
		//[TestCase(247)]
		//[TestCase(248)]
		//[TestCase(249)]
		//[TestCase(250)]
		public void ShouldStopOnDispose(int i)
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Expect(x => x.Invoke("", null)).IgnoreArguments().Repeat.Times(3).Return(_doneTask);
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalSenderForTest(hubConnection, DateTime.UtcNow);
			target.InstantiateBrokerService();
			target.QueueRtaNotification(Guid.Empty, Guid.Empty, new ActualAgentState());
			target.Dispose();
			target.WaitUntilQueueProcessed();

			hubProxy.AssertWasNotCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("NotifyClientsMultiple"), Arg<IEnumerable<Notification>>.Is.Anything));
		}
		
		[Test]
		public void ShouldMakeNewConnectionOnSendExceptionWhileSending_Really()
		{
			Assert.Ignore("Test describing existing functionality");
		}

		[Test]
		public void ShouldIgnoreExceptionsWhenInvoking_Really()
		{
			Assert.Ignore("Test describing existing functionality");
		}
		
		[Test]
		public void ShouldForceRestartHubConnectionWhenNotConnected_Really()
		{
			Assert.Ignore("Test describing existing functionality");
		}

		[Test]
		public void ShouldThrowBrokerNotInstanciatedWhenFailedForceReconnect_Really()
		{
			Assert.Ignore("Test describing existing functionality");
		}

		private class signalSenderForTest : SignalSender
		{
			private readonly IHubConnectionWrapper _hubConnection;
			private readonly DateTime _fakedUtcTime;

			public signalSenderForTest(IHubConnectionWrapper hubConnection, DateTime fakedUtcTime)
				: base(null)
			{
				_hubConnection = hubConnection;
				_fakedUtcTime = fakedUtcTime;
			}

			protected override IHubConnectionWrapper MakeHubConnection()
			{
				return _hubConnection;
			}

			protected override DateTime CurrentUtcTime()
			{
				return _fakedUtcTime;
			}
		}
	}
}
