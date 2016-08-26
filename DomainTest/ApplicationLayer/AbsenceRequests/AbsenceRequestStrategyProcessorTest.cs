using System;
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
		private int _nearFuture;
		private DateTime _nearFutureInterval;
		private DateTime _today;
		private DateTime _farFutureInterval;
		private int _windowSize;
		private DateTimePeriod _nearFuturePeriod;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<AbsenceRequestStrategyProcessor>().For<IAbsenceRequestStrategyProcessor>();
			_nearFuture = 3;
			_windowSize = _nearFuture;
			var now = new DateTime(2016, 03, 01, 10, 0, 0, DateTimeKind.Utc);
			_nearFutureInterval = now.AddMinutes(10*-1);
			_farFutureInterval = now.AddMinutes(20*-1);
			_today = new DateTime(2016, 03, 01, 0, 0, 0, DateTimeKind.Utc);
			_nearFuturePeriod = new DateTimePeriod(_today.AddDays(-1), _today.AddDays(_nearFuture));
		}

		[Test]
		public void ShouldFetchNoAbsenceRequestNewerThan10MinutesForNearFuture()
		{

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 2, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016,03,1,9,58,0, DateTimeKind.Utc),
				PersonRequest = Guid.NewGuid()
			});

			var absenceRequests = Target.Get(_nearFutureInterval, _farFutureInterval, _nearFuturePeriod, _windowSize);
			absenceRequests.Count().Should().Be.EqualTo(0);

		}

		[Test]
		public void ShouldOnlyFetchRequestsInNearFuture()
		{

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 2, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 03, 1, 9, 47, 0, DateTimeKind.Utc),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 5, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 5, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 03, 1, 9, 48, 0, DateTimeKind.Utc),
				PersonRequest = Guid.NewGuid()
			});

			var absenceRequests = Target.Get(_nearFutureInterval, _farFutureInterval, _nearFuturePeriod, _windowSize);
			absenceRequests.Count().Should().Be.EqualTo(1);

		}

		[Test]
		public void ShouldFetchAllNearFutureIfOneIsOlderThan10Minutes()
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 2, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 03, 1, 9, 47, 0, DateTimeKind.Utc),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 2, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 03, 1, 9, 58, 0, DateTimeKind.Utc),
				PersonRequest = Guid.NewGuid()
			});

			
			var absenceRequests = Target.Get(_nearFutureInterval, _farFutureInterval, _nearFuturePeriod, _windowSize);
			absenceRequests.First().Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldFetchFarFutureRequests()
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 5, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 6, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 03, 1, 9, 37, 0, DateTimeKind.Utc),
				PersonRequest = Guid.NewGuid()
			});

			var absenceRequests = Target.Get(_nearFutureInterval, _farFutureInterval, _nearFuturePeriod, _windowSize);
			absenceRequests.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldFetchAllFarFutureIfOneIsOlderThan20Minutes()
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 5, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 6, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 03, 1, 9, 38, 0, DateTimeKind.Utc),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 6, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 7, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 03, 1, 9, 58, 0, DateTimeKind.Utc),
				PersonRequest = Guid.NewGuid()
			});

			var absenceRequests = Target.Get(_nearFutureInterval,_farFutureInterval , _nearFuturePeriod, _windowSize);
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
				Created = new DateTime(2016, 03, 1, 9, 47, 0, DateTimeKind.Utc),
				PersonRequest = farFutureReqId
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 03, 1, 9, 48, 0, DateTimeKind.Utc),
				PersonRequest = nearFutureReqId
			});

		  
			var absenceRequests = Target.Get(_nearFutureInterval, _farFutureInterval, _nearFuturePeriod, _windowSize);
			absenceRequests.Count().Should().Be.EqualTo(1);
			absenceRequests.First().First().Should().Be.EqualTo(nearFutureReqId);
		}

		[Test]
		public void ShouldFetchAllNearFutureIncludedInMaxPeriod()
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 10, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 03, 1, 9, 47, 0, DateTimeKind.Utc),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 7, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3,8, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 03, 1, 9, 58, 0, DateTimeKind.Utc),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 7, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 11, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 03, 1, 9, 58, 0, DateTimeKind.Utc),
				PersonRequest = Guid.NewGuid()
			});


			var absenceRequests = Target.Get(_nearFutureInterval, _farFutureInterval, _nearFuturePeriod, _windowSize);
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
				Created = new DateTime(2016, 03, 1, 8, 47, 0, DateTimeKind.Utc),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 12, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 12, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 03, 1, 8, 46, 0, DateTimeKind.Utc),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 13, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 15, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 03, 1, 8, 58, 0, DateTimeKind.Utc),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 14, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 20, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 03, 1, 8, 58, 0, DateTimeKind.Utc),
				PersonRequest = lastAbsenceReqId
			});


			var absenceRequests = Target.Get(_nearFutureInterval, _farFutureInterval, _nearFuturePeriod, _windowSize);
			absenceRequests.Count().Should().Be.EqualTo(2);
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
				Created = new DateTime(2016, 03, 1, 9, 47, 0, DateTimeKind.Utc),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 7, 27, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 03, 1, 8, 58, 0, DateTimeKind.Utc),
				PersonRequest = Guid.NewGuid()
			});

			var absenceRequests = Target.Get(_nearFutureInterval, _farFutureInterval, _nearFuturePeriod, _windowSize);
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
				Created = new DateTime(2016, 03, 1, 8, 58, 0, DateTimeKind.Utc),
				PersonRequest = pastId
			});

			var absenceRequests = Target.Get(_nearFutureInterval, _farFutureInterval, _nearFuturePeriod, _windowSize);
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
				Created = new DateTime(2016, 03, 1, 8, 58, 0, DateTimeKind.Utc),
				PersonRequest = pastId
			});

			var futureId = Guid.NewGuid();

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 2, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 03, 1, 8, 58, 0, DateTimeKind.Utc),
				PersonRequest = futureId
			});

			var absenceRequests = Target.Get(_nearFutureInterval, _farFutureInterval, _nearFuturePeriod, _windowSize);
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
				Created = new DateTime(2016, 03, 1, 9, 58, 0, DateTimeKind.Utc),
				PersonRequest = pastId
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 5, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 03, 1, 9, 58, 0, DateTimeKind.Utc),
				PersonRequest = Guid.NewGuid()
			});

			var futureId = Guid.NewGuid();

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 6, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 7, 23, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 03, 1, 9, 58, 0, DateTimeKind.Utc),
				PersonRequest = futureId
			});

			var absenceRequests = Target.Get(_nearFutureInterval, _farFutureInterval, _nearFuturePeriod, _windowSize);
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
				Created = new DateTime(2016, 03, 1, 9, 38, 0, DateTimeKind.Utc),
				PersonRequest = yesterdayId
			});

			//Add far future to make sure yesterday is not picked up as a Past Request
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 3, 5, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 3, 5, 19, 59, 00, DateTimeKind.Utc),
				Created = new DateTime(2016, 03, 1, 9, 38, 0, DateTimeKind.Utc),
				PersonRequest = fatFutureId
			});

			var absenceRequests = Target.Get(_nearFutureInterval, _farFutureInterval, _nearFuturePeriod, _windowSize);
			absenceRequests.First().Count().Should().Be.EqualTo(1);
			absenceRequests.First().First().Should().Be.EqualTo(yesterdayId);
		}
	}

	
}
