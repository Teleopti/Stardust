﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.IocCommon;
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

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<AbsenceRequestStrategyProcessor>().For<IAbsenceRequestStrategyProcessor>();
			_windowSize = 3;
			_now = new DateTime(2016, 03, 01, 10, 0, 0, DateTimeKind.Utc);
			_pastThreshold = _now;
			_nearFutureThreshold = _now.AddMinutes(-10);
			_farFutureThreshold = _now.AddMinutes(-20);
			_initialPeriod = new DateOnlyPeriod(new DateOnly(_now.AddDays(-1)), new DateOnly(_now.AddDays(_windowSize)));
		}

		[Test]
		public void ShouldFetchNoAbsenceRequestNewerThan10MinutesForNearFuture()
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 2, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-2),
				PersonRequest = Guid.NewGuid()
			});

			var absenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			absenceRequests.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldOnlyFetchRequestsInNearFuture()
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 2, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-13),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 5, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 5, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-12),
				PersonRequest = Guid.NewGuid()
			});

			var absenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			absenceRequests.Count.Should().Be.EqualTo(1);
		}

		[Test, Ignore("Ziggy - please review this case for bug #41788!")]
		public void ShouldNotBlockFurtherProcessingWhenSentRequestsAreStuckOnQueue()
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 11, 14, 16, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 11, 14, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 11, 14, 14, 0, 0, DateTimeKind.Utc),
				PersonRequest = Guid.NewGuid(),
				Sent = new DateTime(2016, 11, 14, 14, 30, 0, DateTimeKind.Utc),
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 11, 20, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 11, 20, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 11, 20, 4, 0, 0, DateTimeKind.Utc),
				PersonRequest = Guid.NewGuid()
			});

			var absenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			absenceRequests.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldFetchAllNearFutureIfOneIsOlderThan10Minutes()
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 2, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-12),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 2, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-2),
				PersonRequest = Guid.NewGuid()
			});

			var absenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			absenceRequests.First().Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldFetchFarFutureRequests()
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 5, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 6, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-23),
				PersonRequest = Guid.NewGuid()
			});

			var absenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			absenceRequests.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldFetchAllFarFutureIfOneIsOlderThan20Minutes()
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 5, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 6, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-22),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 6, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 7, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-2),
				PersonRequest = Guid.NewGuid()
			});

			var absenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			absenceRequests.First().Count().Should().Be.EqualTo(2);
		}


		[Test]
		public void ShouldFetchNearFutureRequestsBeforeFarFuture()
		{
			var nearFutureReqId = Guid.NewGuid();
			var farFutureReqId = Guid.NewGuid();
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 5, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 6, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-13),
				PersonRequest = farFutureReqId
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-12),
				PersonRequest = nearFutureReqId
			});

			var absenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			absenceRequests.Count.Should().Be.EqualTo(1);
			absenceRequests.First().First().Should().Be.EqualTo(nearFutureReqId);
		}

		[Test]
		public void ShouldFetchAllNearFutureIncludedInMaxPeriod()
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 10, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-13),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 7, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 8, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-2),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 7, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 11, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-2),
				PersonRequest = Guid.NewGuid()
			});

			var absenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			absenceRequests.First().Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldFetchFarFutureAbsencesUsingSlidingWindow()
		{
			var lastAbsenceReqId = Guid.NewGuid();
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 11, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 13, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-13).AddHours(-1),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 12, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 12, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-14).AddHours(-1),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 13, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 15, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-2).AddHours(-1),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 14, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 20, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-2).AddHours(-1),
				PersonRequest = lastAbsenceReqId
			});

			var absenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			absenceRequests.Count.Should().Be.EqualTo(2);
			absenceRequests.First().Count().Should().Be.EqualTo(3);
			absenceRequests.Second().Count().Should().Be.EqualTo(1);
			absenceRequests.Second().First().Should().Be.EqualTo(lastAbsenceReqId);
		}


		[Test]
		public void ExcludeRequestsWithPeriodLongerThanConfiguredDays()
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 10, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-13),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 7, 27, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-2).AddHours(-1),
				PersonRequest = Guid.NewGuid()
			});

			var absenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			absenceRequests.First().Count().Should().Be.EqualTo(1);
		}


		[Test]
		public void FetchPastRequestsIfNoOtherRequestsInQueue()
		{
			var pastId = Guid.NewGuid();

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 2, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 2, 2, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-2).AddHours(-1),
				PersonRequest = pastId
			});

			var absenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			absenceRequests.First().First().Should().Be.EqualTo(pastId);
		}


		[Test]
		public void DontFetchPastRequestsIfOtherRequestsInQueue()
		{
			var pastId = Guid.NewGuid();

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 2, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 2, 2, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-2).AddHours(-1),
				PersonRequest = pastId
			});

			var futureId = Guid.NewGuid();

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 2, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-2).AddHours(-1),
				PersonRequest = futureId
			});

			var absenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			absenceRequests.First().Count().Should().Be.EqualTo(1);
			absenceRequests.First().First().Should().Be.EqualTo(futureId);
		}

		[Test]
		public void DontProcessRecentReuqestIfThereExistsAtleastOnePastPresentFuture()
		{
			var pastId = Guid.NewGuid();

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 2, 25, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 2, 26, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-2),
				PersonRequest = pastId
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 5, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-2),
				PersonRequest = Guid.NewGuid()
			});

			var futureId = Guid.NewGuid();

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 6, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 7, 23, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-2),
				PersonRequest = futureId
			});

			var absenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			absenceRequests.First().Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPickUpYesterdayPeriodRequestsAsNearFuture()
		{
			var yesterdayId = Guid.NewGuid();
			var fatFutureId = Guid.NewGuid();

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 2, 29, 20, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 1, 19, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-22),
				PersonRequest = yesterdayId
			});

			//Add far future to make sure yesterday is not picked up as a Past Request
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 5, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 5, 19, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-22),
				PersonRequest = fatFutureId
			});

			var absenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			absenceRequests.First().Count().Should().Be.EqualTo(1);
			absenceRequests.First().First().Should().Be.EqualTo(yesterdayId);
		}

		[Test]
		public void ShouldNotSendRequestInProcessingPeriod()
		{
			var id = Guid.NewGuid();
			var timeStamp1 = _now.AddMinutes(-10);

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 5, 8, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 5, 19, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-22),
				PersonRequest = Guid.NewGuid(),
				Sent = timeStamp1
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 6, 8, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 6, 19, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-22),
				PersonRequest = Guid.NewGuid(),
				Sent = timeStamp1
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 9, 8, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 9, 19, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-22),
				PersonRequest = Guid.NewGuid(),
				Sent = new DateTime(2016, 03, 01, 9, 52, 0, DateTimeKind.Utc)
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 6, 15, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 7, 19, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-22),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 8, 8, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 8, 19, 59, 00, DateTimeKind.Utc),
				Created = _now.AddMinutes(-22),
				PersonRequest = id
			});

			var absenceRequests = Target.Get(_nearFutureThreshold, _farFutureThreshold, _pastThreshold, _initialPeriod, _windowSize);
			absenceRequests.Count.Should().Be.EqualTo(1);
			absenceRequests.First().Count().Should().Be.EqualTo(1);
			absenceRequests.First().First().Should().Be.EqualTo(id);
		}
	}
}
