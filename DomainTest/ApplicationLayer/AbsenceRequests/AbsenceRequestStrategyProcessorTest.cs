using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[TestFixture]
	public class AbsenceRequestStrategyProcessorTest : ISetup
	{
		public FakeQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public IAbsenceRequestStrategyProcessor Target;
		private DateTime _nearFutureThreshold;
		private DateTime _farFutureThreshold;
		private DateTime _pastThreshold;
		private int _windowSize;
		private DateOnlyPeriod _initialPeriod;
		private DateTime _now;
		public FakeConfigReader ConfigReader;
		public FakeCommandDispatcher CommandDispatcher;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<AbsenceRequestStrategyProcessor>().For<IAbsenceRequestStrategyProcessor>();
			system.UseTestDouble<FakeCommandDispatcher>().For<ICommandDispatcher>();
			_windowSize = 3;
			_now = new DateTime(2016, 03, 01, 0, 0, 0, DateTimeKind.Utc);
			_pastThreshold = _now;
			_nearFutureThreshold = _now.AddMinutes(-10);
			_farFutureThreshold = _now.AddMinutes(-20);
			_initialPeriod = new DateOnlyPeriod(new DateOnly(_now.AddDays(-1)), new DateOnly(_now.AddDays(_windowSize)));
			system.UseTestDouble<FilterRequestsWithDifferentVersion41930ToggleOff>().For<IFilterRequestsWithDifferentVersion>();
		}

		[Test]
		public void ShouldFetchNoAbsenceRequestNewerThan10MinutesForNearFuture()
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(1),
				EndDateTime = _now.AddDays(1).AddHours(1),
				Created = _now.AddMinutes(-2),
				PersonRequest = Guid.NewGuid()
			});

			var queuedAbsenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldOnlyFetchRequestsInNearFuture()
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(1),
				EndDateTime = _now.AddDays(1).AddHours(1),
				Created = _now.AddMinutes(-13),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(4),
				EndDateTime = _now.AddDays(4).AddHours(1),
				Created = _now.AddMinutes(-12),
				PersonRequest = Guid.NewGuid()
			});

			var queuedAbsenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(1);
		}
		
		[Test]
		public void ShouldFetchAllNearFutureIfOneIsOlderThan10Minutes()
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(1),
				EndDateTime = _now.AddDays(1).AddHours(1),
				Created = _now.AddMinutes(-12),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(1),
				EndDateTime = _now.AddDays(1).AddHours(1),
				Created = _now.AddMinutes(-2),
				PersonRequest = Guid.NewGuid()
			});

			var queuedAbsenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			queuedAbsenceRequests.First().Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldFetchFarFutureRequests()
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(4),
				EndDateTime = _now.AddDays(4).AddHours(1),
				Created = _now.AddMinutes(-23),
				PersonRequest = Guid.NewGuid()
			});

			var queuedAbsenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldFetchAllFarFutureIfOneIsOlderThan20Minutes()
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(4),
				EndDateTime = _now.AddDays(4).AddHours(1),
				Created = _now.AddMinutes(-22),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(5),
				EndDateTime = _now.AddDays(5).AddHours(1),
				Created = _now.AddMinutes(-2),
				PersonRequest = Guid.NewGuid()
			});

			var queuedAbsenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			queuedAbsenceRequests.First().Count().Should().Be.EqualTo(2);
		}


		[Test]
		public void ShouldFetchNearFutureRequestsBeforeFarFuture()
		{
			var nearFutureReqId = Guid.NewGuid();
			var farFutureReqId = Guid.NewGuid();
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(4),
				EndDateTime = _now.AddDays(4).AddHours(1),
				Created = _now.AddMinutes(-13),
				PersonRequest = farFutureReqId
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(1),
				EndDateTime = _now.AddDays(1).AddHours(1),
				Created = _now.AddMinutes(-12),
				PersonRequest = nearFutureReqId
			});

			var queuedAbsenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(1);
			queuedAbsenceRequests.First().First().PersonRequest.Should().Be.EqualTo(nearFutureReqId);
		}

		[Test]
		public void ShouldFetchAllNearFutureIncludedInMaxPeriod()
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(1),
				EndDateTime = _now.AddDays(9).AddHours(1),
				Created = _now.AddMinutes(-13),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(2),
				EndDateTime = _now.AddDays(7).AddHours(1),
				Created = _now.AddMinutes(-2),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(6),
				EndDateTime = _now.AddDays(10).AddHours(1),
				Created = _now.AddMinutes(-2),
				PersonRequest = Guid.NewGuid()
			});

			var queuedAbsenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			queuedAbsenceRequests.First().Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldFetchFarFutureAbsencesUsingSlidingWindow()
		{
			var lastAbsenceReqId = Guid.NewGuid();
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(10),
				EndDateTime = _now.AddDays(12).AddHours(1),
				Created = _now.AddMinutes(-13).AddHours(-1),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(11),
				EndDateTime = _now.AddDays(11).AddHours(1),
				Created = _now.AddMinutes(-14).AddHours(-1),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(12),
				EndDateTime = _now.AddDays(14).AddHours(1),
				Created = _now.AddMinutes(-2).AddHours(-1),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(13),
				EndDateTime = _now.AddDays(19).AddHours(1),
				Created = _now.AddMinutes(-2).AddHours(-1),
				PersonRequest = lastAbsenceReqId
			});

			var queuedAbsenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(2);
			queuedAbsenceRequests.First().Count().Should().Be.EqualTo(3);
			queuedAbsenceRequests.Second().Count().Should().Be.EqualTo(1);
			queuedAbsenceRequests.Second().First().PersonRequest.Should().Be.EqualTo(lastAbsenceReqId);
		}


		[Test]
		public void ExcludeRequestsWithPeriodLongerThanConfiguredDays()
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(1),
				EndDateTime = _now.AddDays(9).AddHours(1),
				Created = _now.AddMinutes(-13),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(1),
				EndDateTime = _now.AddDays(66),
				Created = _now.AddMinutes(-2).AddHours(-1),
				PersonRequest = Guid.NewGuid()
			});

			var queuedAbsenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			queuedAbsenceRequests.First().Count().Should().Be.EqualTo(1);
		}


		[Test]
		public void FetchPastRequestsIfNoOtherRequestsInQueue()
		{
			var pastId = Guid.NewGuid();

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(-10),
				EndDateTime = _now.AddDays(-10).AddHours(1),
				Created = _now.AddMinutes(-2).AddHours(-1),
				PersonRequest = pastId
			});

			var queuedAbsenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			queuedAbsenceRequests.First().First().PersonRequest.Should().Be.EqualTo(pastId);
		}


		[Test]
		public void DontFetchPastRequestsIfOtherRequestsInQueue()
		{
			var pastId = Guid.NewGuid();

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(-10),
				EndDateTime = _now.AddDays(-10).AddHours(1),
				Created = _now.AddMinutes(-2).AddHours(-1),
				PersonRequest = pastId
			});

			var futureId = Guid.NewGuid();

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(1),
				EndDateTime = _now.AddDays(1).AddHours(1),
				Created = _now.AddMinutes(-2).AddHours(-1),
				PersonRequest = futureId
			});

			var queuedAbsenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			queuedAbsenceRequests.First().Count().Should().Be.EqualTo(1);
			queuedAbsenceRequests.First().First().PersonRequest.Should().Be.EqualTo(futureId);
		}

		[Test]
		public void DontProcessRecentReuqestIfThereExistsAtleastOnePastPresentFuture()
		{
			var pastId = Guid.NewGuid();

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(-10),
				EndDateTime = _now.AddDays(-10).AddHours(1),
				Created = _now.AddMinutes(-2),
				PersonRequest = pastId
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(1),
				EndDateTime = _now.AddDays(1).AddHours(1),
				Created = _now.AddMinutes(-2),
				PersonRequest = Guid.NewGuid()
			});

			var futureId = Guid.NewGuid();

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(5),
				EndDateTime = _now.AddDays(5).AddHours(1),
				Created = _now.AddMinutes(-2),
				PersonRequest = futureId
			});

			var queuedAbsenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			queuedAbsenceRequests.First().Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPickUpYesterdayPeriodRequestsAsNearFuture()
		{
			var yesterdayId = Guid.NewGuid();
			var fatFutureId = Guid.NewGuid();

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(-1).AddHours(1),
				EndDateTime = _now.AddDays(-1).AddHours(2),
				Created = _now.AddMinutes(-22),
				PersonRequest = yesterdayId
			});

			//Add far future to make sure yesterday is not picked up as a Past Request
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(4),
				EndDateTime = _now.AddDays(4).AddHours(1),
				Created = _now.AddMinutes(-22),
				PersonRequest = fatFutureId
			});

			var queuedAbsenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			queuedAbsenceRequests.First().Count().Should().Be.EqualTo(1);
			queuedAbsenceRequests.First().First().PersonRequest.Should().Be.EqualTo(yesterdayId);
		}

		[Test]
		public void ShouldNotSendRequestInProcessingPeriod()
		{
			var id = Guid.NewGuid();
			var timeStamp1 = _now.AddMinutes(-10);

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(4),
				EndDateTime = _now.AddDays(4).AddHours(1),
				Created = _now.AddMinutes(-22),
				PersonRequest = Guid.NewGuid(),
				Sent = timeStamp1
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(5),
				EndDateTime = _now.AddDays(5).AddHours(1),
				Created = _now.AddMinutes(-22),
				PersonRequest = Guid.NewGuid(),
				Sent = timeStamp1
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(8),
				EndDateTime = _now.AddDays(8).AddHours(1),
				Created = _now.AddMinutes(-22),
				PersonRequest = Guid.NewGuid(),
				Sent = _now.AddMinutes(-8)
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(5),
				EndDateTime = _now.AddDays(6).AddHours(1),
				Created = _now.AddMinutes(-22),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(7),
				EndDateTime = _now.AddDays(7).AddHours(1),
				Created = _now.AddMinutes(-22),
				PersonRequest = id
			});

			var queuedAbsenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(1);
			queuedAbsenceRequests.First().Count().Should().Be.EqualTo(1);
			queuedAbsenceRequests.First().First().PersonRequest.Should().Be.EqualTo(id);
		}

		[Test]
		public void ShouldIgnoreRequestThatAreLongerThanMaximumDays()
		{
			ConfigReader.FakeSetting("MaximumDayLengthForAbsenceRequest", "20");
			var id = Guid.NewGuid();
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 4, 2, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-13),
				PersonRequest = id
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 5, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 5, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-12),
				PersonRequest = Guid.NewGuid()
			});

			var queuedAbsenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldDenyAbsenceThatAreLongerThanConfiguredDays()
		{
			ConfigReader.FakeSetting("MaximumDayLengthForAbsenceRequest", "20");
			var id = Guid.NewGuid();
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 4, 2, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-13),
				PersonRequest = id
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 5, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 5, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-12),
				PersonRequest = Guid.NewGuid()
			});

			Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			((DenyRequestCommand) CommandDispatcher.LatestCommand).PersonRequestId.Should().Be.EqualTo(id);
		}

		[Test]
		public void ShouldRemoveAbsenceThatAreLongerThanConfiguredDays()
		{
			ConfigReader.FakeSetting("MaximumDayLengthForAbsenceRequest", "20");
			var id = Guid.NewGuid();
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 4, 2, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-13),
				PersonRequest = id
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 5, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 5, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-12),
				PersonRequest = Guid.NewGuid()
			});

			Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			QueuedAbsenceRequestRepository.LoadAll().Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldIncludePlaceholderRequests()
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 2, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-20),
				PersonRequest = Guid.Empty
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 2, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-21),
				PersonRequest = Guid.NewGuid()
			});

			var queuedAbsenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(1);
			queuedAbsenceRequests.First().Select(x => x.PersonRequest).Should().Contain(Guid.Empty);
		}
	}
}
