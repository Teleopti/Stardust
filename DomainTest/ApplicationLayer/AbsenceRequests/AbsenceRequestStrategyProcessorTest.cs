using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[TestFixture]
	public class AbsenceRequestStrategyProcessorTest : IIsolateSystem
	{
		public FakeQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public IAbsenceRequestStrategyProcessor Target;
		private DateTime _thresholdTime;
		private DateTime _pastThreshold;
		private int _windowSize;
		private DateOnlyPeriod _initialPeriod;
		private DateTime _now;
		public FakeConfigReader ConfigReader;
		public FakePersonRequestRepository PersonRequestRepository;
		public MutableNow Now;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<AbsenceRequestStrategyProcessor>().For<IAbsenceRequestStrategyProcessor>();
			_windowSize = 2;
			_now = new DateTime(2016, 03, 01, 0, 0, 0, DateTimeKind.Utc);
			_pastThreshold = _now;
			_thresholdTime = _now.AddMinutes(-10);
			_initialPeriod = new DateOnlyPeriod(new DateOnly(_now.AddDays(-1)), new DateOnly(_now.AddDays(_windowSize)));
			isolate.UseTestDouble<FilterRequestsWithDifferentVersion41930ToggleOff>().For<IFilterRequestsWithDifferentVersion>();
		}

		[Test]
		public void ShouldNotFetchRequestNewerThan10MinutesForNearFuture()
		{
			var now = new DateTime(2018, 05, 31, 16, 0, 0, DateTimeKind.Utc);
			Now.Is(now);
			var pastThreshold = now;
			var thresholdTime = now.AddMinutes(-10);
			var initialPeriod = new DateOnlyPeriod(new DateOnly(now.AddDays(-1)), new DateOnly(now.AddDays(_windowSize)));

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2018, 06, 1, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2018, 06, 1, 1, 0, 0, DateTimeKind.Utc),
				Created = now.AddMinutes(-2),
				PersonRequest = Guid.NewGuid()
			});

			var queuedAbsenceRequests = Target.Get(thresholdTime, pastThreshold, initialPeriod, _windowSize);
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

			var queuedAbsenceRequests = Target.Get(_thresholdTime, _pastThreshold, _initialPeriod, _windowSize);
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

			var queuedAbsenceRequests = Target.Get(_thresholdTime, _pastThreshold, _initialPeriod, _windowSize);
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

			var queuedAbsenceRequests = Target.Get(_thresholdTime, _pastThreshold, _initialPeriod, _windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldFetchAllFarFutureIfOneIsOlderThan10MinutesWithWindow3()
		{
			var windowSize = 3;
			var initialPeriod = new DateOnlyPeriod(new DateOnly(_now.AddDays(-1)), new DateOnly(_now.AddDays(windowSize)));
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(4),
				EndDateTime = _now.AddDays(4).AddHours(1),
				Created = _now.AddMinutes(-12),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = _now.AddDays(5),
				EndDateTime = _now.AddDays(5).AddHours(1),
				Created = _now.AddMinutes(-2),
				PersonRequest = Guid.NewGuid()
			});

			var queuedAbsenceRequests = Target.Get(_thresholdTime, _pastThreshold, initialPeriod, windowSize);
			queuedAbsenceRequests.First().Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldFetchAllFarFutureIfOneIsOlderThan10MinutesWithWindow2()
		{
			var now = new DateTime(2016, 03, 01, 0, 0, 0, DateTimeKind.Utc);
			Now.Is(now);
			var pastThreshold = now;
			var thresholdTime = now.AddMinutes(-10);
			var windowSize = 2;
			var initialPeriod = new DateOnlyPeriod(new DateOnly(now.AddDays(-1)), new DateOnly(now.AddDays(windowSize)));

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 03, 05, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 03, 05, 1, 0, 0, DateTimeKind.Utc),
				Created = now.AddMinutes(-12),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 03, 06, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 03, 06, 1, 0, 0, DateTimeKind.Utc),
				Created = now.AddMinutes(-2),
				PersonRequest = Guid.NewGuid()
			});

			var queuedAbsenceRequests = Target.Get(thresholdTime, pastThreshold, initialPeriod, windowSize);
			queuedAbsenceRequests.Count().Should().Be.EqualTo(2);
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

			var queuedAbsenceRequests = Target.Get(_thresholdTime, _pastThreshold, _initialPeriod, _windowSize);
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

			var queuedAbsenceRequests = Target.Get(_thresholdTime, _pastThreshold, _initialPeriod, _windowSize);
			queuedAbsenceRequests.First().Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldFetchFarFutureRequestsUsingSlidingWindowSize3()
		{
			var now = new DateTime(2016, 03, 01, 0, 0, 0, DateTimeKind.Utc);
			Now.Is(now);
			var pastThreshold = now;
			var thresholdTime = now.AddMinutes(-10);
			var windowSize = 3;
			var initialPeriod = new DateOnlyPeriod(new DateOnly(now.AddDays(-1)), new DateOnly(now.AddDays(windowSize)));

			var lastAbsenceReqId = Guid.NewGuid();
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 03, 11, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 03, 13, 1, 0, 0, DateTimeKind.Utc),
				Created = _now.AddMinutes(-13).AddHours(-1),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 03, 12, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 03, 12, 1, 0, 0, DateTimeKind.Utc),
				Created = _now.AddMinutes(-14).AddHours(-1),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 03, 13, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 03, 13, 10, 0, 0, DateTimeKind.Utc),
				Created = _now.AddMinutes(-2).AddHours(-1),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2016, 03, 14, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 03, 20, 0, 0, 0, DateTimeKind.Utc).AddHours(1),
				Created = _now.AddMinutes(-2).AddHours(-1),
				PersonRequest = lastAbsenceReqId
			});

			var queuedAbsenceRequests = Target.Get(thresholdTime, pastThreshold, initialPeriod, windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(2);
			queuedAbsenceRequests.First().Count().Should().Be.EqualTo(3);
			queuedAbsenceRequests.Second().Count().Should().Be.EqualTo(1);
			queuedAbsenceRequests.Second().First().PersonRequest.Should().Be.EqualTo(lastAbsenceReqId);
		}

		[Test]
		public void ShouldFetchFarFutureRequestsUsingSlidingWindowSize2()
		{
			var now = new DateTime(2018, 6, 7, 0, 0, 0, DateTimeKind.Utc);
			Now.Is(now);
			var pastThreshold = now;
			var thresholdTime = now.AddMinutes(-10);
			var windowSize = 2;
			var initialPeriod = new DateOnlyPeriod(new DateOnly(now.AddDays(-1)), new DateOnly(now.AddDays(windowSize)));

			var lastAbsenceReqId = Guid.NewGuid();
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2018, 6, 10, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2018, 6, 12, 0, 0, 0, DateTimeKind.Utc).AddHours(1),
				Created = now.AddMinutes(-13).AddHours(-1),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2018, 6, 11, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2018, 6, 11, 1, 0, 0, DateTimeKind.Utc),
				Created = now.AddMinutes(-14).AddHours(-1),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2018, 6, 12, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2018, 6, 14, 1, 0, 0, DateTimeKind.Utc),
				Created = now.AddMinutes(-2).AddHours(-1),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2018, 6, 13, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2018, 6, 19, 1, 0, 0, DateTimeKind.Utc),
				Created = now.AddMinutes(-2).AddHours(-1),
				PersonRequest = lastAbsenceReqId
			});

			var queuedAbsenceRequests = Target.Get(thresholdTime, pastThreshold, initialPeriod, windowSize);
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

			var queuedAbsenceRequests = Target.Get(_thresholdTime, _pastThreshold, _initialPeriod, _windowSize);
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

			var queuedAbsenceRequests = Target.Get(_thresholdTime, _pastThreshold, _initialPeriod, _windowSize);
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

			var queuedAbsenceRequests = Target.Get(_thresholdTime, _pastThreshold, _initialPeriod, _windowSize);
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
				Created = _now.AddDays(-10),
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

			var queuedAbsenceRequests = Target.Get(_thresholdTime, _pastThreshold, _initialPeriod, _windowSize);
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

			var queuedAbsenceRequests = Target.Get(_thresholdTime, _pastThreshold, _initialPeriod, _windowSize);
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

			var queuedAbsenceRequests = Target.Get(_thresholdTime, _pastThreshold, _initialPeriod, _windowSize);
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

			var queuedAbsenceRequests = Target.Get(_thresholdTime, _pastThreshold, _initialPeriod, _windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldDenyAbsenceThatAreLongerThanConfiguredDays()
		{
			ConfigReader.FakeSetting("MaximumDayLengthForAbsenceRequest", "20");

			var longPeriodPersonRequest = createAbsenceRequest(new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 4, 2, 23, 59, 00, DateTimeKind.Utc));
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = longPeriodPersonRequest.Request.Period.StartDateTime,
				EndDateTime = longPeriodPersonRequest.Request.Period.EndDateTime,
				Created = _now.AddMinutes(-13),
				PersonRequest = longPeriodPersonRequest.Id.Value
			});

			var normalPersonRequest = createAbsenceRequest(new DateTime(2016, 3, 5, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 3, 5, 23, 59, 00, DateTimeKind.Utc));
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = normalPersonRequest.Request.Period.StartDateTime,
				EndDateTime = normalPersonRequest.Request.Period.EndDateTime,
				Created = _now.AddMinutes(-12),
				PersonRequest = normalPersonRequest.Id.Value
			});

			Target.Get(_thresholdTime, _pastThreshold, _initialPeriod, _windowSize);
			longPeriodPersonRequest.IsDenied.Should().Be(true);
			longPeriodPersonRequest.DenyReason.Should().Be("The requested period is too long");
			normalPersonRequest.IsPending.Should().Be(true);
		}

		[Test]
		public void ShouldNotSetAbsenceRequestAsWaitlistedThatAreLongerThanConfiguredDays()
		{
			Now.Is(_now);

			ConfigReader.FakeSetting("MaximumDayLengthForAbsenceRequest", "20");

			var longPeriodPersonRequest = createAbsenceRequest(new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 4, 2, 23, 59, 00, DateTimeKind.Utc));
			var person = longPeriodPersonRequest.Person;

			var workflowControlSet = new WorkflowControlSet()
			{
				AbsenceRequestWaitlistEnabled = true
			};
			var absence = (longPeriodPersonRequest.Request as IAbsenceRequest).Absence;
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				AbsenceRequestProcess = new GrantAbsenceRequest(),
				Period = new DateOnlyPeriod(new DateOnly(_now.Date), new DateOnly(_now.Date.AddDays(40))),
				OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(_now.Date), new DateOnly(_now.Date.AddDays(40)))
			});

			longPeriodPersonRequest.Person.WorkflowControlSet = workflowControlSet;

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = longPeriodPersonRequest.Request.Period.StartDateTime,
				EndDateTime = longPeriodPersonRequest.Request.Period.EndDateTime,
				Created = _now.AddMinutes(-13),
				PersonRequest = longPeriodPersonRequest.Id.Value
			});

			Target.Get(_thresholdTime, _pastThreshold, _initialPeriod, _windowSize);
			longPeriodPersonRequest.IsDenied.Should().Be(true);
			longPeriodPersonRequest.DenyReason.Should().Be("The requested period is too long");
			longPeriodPersonRequest.IsWaitlisted.Should().Be(false);
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

			Target.Get(_thresholdTime, _pastThreshold, _initialPeriod, _windowSize);
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

			var queuedAbsenceRequests = Target.Get(_thresholdTime, _pastThreshold, _initialPeriod, _windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(1);
			queuedAbsenceRequests.First().Select(x => x.PersonRequest).Should().Contain(Guid.Empty);
		}

		[Test]
		public void ShouldFetchOverlappingRequestsForNearFuture()
		{
			var now = new DateTime(2018, 05, 31, 16, 0, 0, DateTimeKind.Utc);
			var pastThreshold = now;
			var thresholdTime = now.AddMinutes(-10);
			var windowSize = 2;
			var initialPeriod = new DateOnlyPeriod(new DateOnly(now.AddDays(-1)), new DateOnly(now.AddDays(windowSize)));

			Now.Is(now);

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2018, 6, 1, 10, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2018, 6, 3, 10, 0, 00, DateTimeKind.Utc),
				Created = now.AddMinutes(-20),
				PersonRequest = Guid.Empty
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2018, 6, 3, 9, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2018, 6, 3, 11, 0, 00, DateTimeKind.Utc),
				Created = now.AddMinutes(-21),
				PersonRequest = Guid.NewGuid()
			});

			var queuedAbsenceRequests = Target.Get(thresholdTime, pastThreshold, initialPeriod, windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(1);
			queuedAbsenceRequests.First().Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldFetchMultiOverlappingRequestsForNearFutureOnDifferentWindows()
		{
			var now = new DateTime(2018, 05, 31, 16, 0, 0, DateTimeKind.Utc);
			var pastThreshold = now;
			var thresholdTime = now.AddMinutes(-10);
			var windowSize = 2;
			var initialPeriod = new DateOnlyPeriod(new DateOnly(now.AddDays(-1)), new DateOnly(now.AddDays(windowSize)));

			Now.Is(now);

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2018, 6, 1, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2018, 6, 3, 0, 0, 00, DateTimeKind.Utc),
				Created = now.AddMinutes(-20),
				PersonRequest = Guid.Empty
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2018, 6, 3, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2018, 6, 8, 9, 0, 00, DateTimeKind.Utc),
				Created = now.AddMinutes(-21),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2018, 6, 5, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2018, 6, 6, 0, 0, 00, DateTimeKind.Utc),
				Created = now.AddMinutes(-21),
				PersonRequest = Guid.NewGuid()
			});

			var queuedAbsenceRequests = Target.Get(thresholdTime, pastThreshold, initialPeriod, windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(1);
			queuedAbsenceRequests.First().Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldFetchMultiOverlappingRequestsForfarFutureOnDifferentWindows()
		{
			var now = new DateTime(2018, 05, 27, 16, 0, 0, DateTimeKind.Utc);
			var pastThreshold = now;
			var thresholdTime = now.AddMinutes(-10);
			var windowSize = 2;
			var initialPeriod = new DateOnlyPeriod(new DateOnly(now.AddDays(-1)), new DateOnly(now.AddDays(windowSize)));

			Now.Is(now);

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2018, 6, 1, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2018, 6, 3, 0, 0, 00, DateTimeKind.Utc),
				Created = now.AddMinutes(-20),
				PersonRequest = Guid.Empty
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2018, 6, 3, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2018, 6, 8, 9, 0, 00, DateTimeKind.Utc),
				Created = now.AddMinutes(-21),
				PersonRequest = Guid.NewGuid()
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2018, 6, 5, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2018, 6, 6, 0, 0, 00, DateTimeKind.Utc),
				Created = now.AddMinutes(-21),
				PersonRequest = Guid.NewGuid()
			});

			var queuedAbsenceRequests = Target.Get(thresholdTime, pastThreshold, initialPeriod, windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(2);
			queuedAbsenceRequests.First().Count().Should().Be.EqualTo(2);
			queuedAbsenceRequests.Second().Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldFetchOverlappingRequestsForFarFuture()
		{
			var now = new DateTime(2018, 05, 31, 16, 0, 0, DateTimeKind.Utc);
			var pastThreshold = now;
			var thresholdTime = now.AddMinutes(-10);
			var windowSize = 2;
			var initialPeriod = new DateOnlyPeriod(new DateOnly(now.AddDays(-1)), new DateOnly(now.AddDays(windowSize)));

			Now.Is(now);

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2018, 6, 3, 10, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2018, 6, 5, 10, 0, 00, DateTimeKind.Utc),
				Created = now.AddMinutes(-20),
				PersonRequest = Guid.Empty
			});

			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				StartDateTime = new DateTime(2018, 6, 5, 9, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2018, 6, 7, 11, 0, 00, DateTimeKind.Utc),
				Created = now.AddMinutes(-21),
				PersonRequest = Guid.NewGuid()
			});

			var queuedAbsenceRequests = Target.Get(thresholdTime, pastThreshold, initialPeriod, windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(1);
			queuedAbsenceRequests.First().Count().Should().Be.EqualTo(2);
		}

		private IPersonRequest createAbsenceRequest(DateTime startDateTime, DateTime endDateTime)
		{
			var personRequestFactory = new PersonRequestFactory();
			var absenceRequest = personRequestFactory.CreateAbsenceRequest(new Absence().WithId()
					, new DateTimePeriod(startDateTime, endDateTime))
				.WithId();
			var personRequest = (absenceRequest.Parent as IPersonRequest).WithId();
			PersonRequestRepository.Add(personRequest);
			return personRequest;
		}
	}
}
