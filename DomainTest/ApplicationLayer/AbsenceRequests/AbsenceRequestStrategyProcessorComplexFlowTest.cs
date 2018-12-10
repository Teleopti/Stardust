using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[TestFixture]
	public class AbsenceRequestStrategyProcessorComplexFlowTest : IIsolateSystem
	{
		public FakeQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public IAbsenceRequestStrategyProcessor Target;
		private int _windowSize;
		public FakeConfigReader ConfigReader;
		public FakePersonRequestRepository PersonRequestRepository;
		public MutableNow Now;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<AbsenceRequestStrategyProcessor>().For<IAbsenceRequestStrategyProcessor>();
			_windowSize = 2;
			isolate.UseTestDouble<FilterRequestsWithDifferentVersion41930ToggleOff>().For<IFilterRequestsWithDifferentVersion>();
		}

		//This test is to prove that 10 min wont work
		[Test]
		public void FetchSingleNearFutureRequestEvenFarFutureExists()
		{
			var now = new DateTime(2018, 07, 09, 9, 19, 0, DateTimeKind.Utc);
			var pastThreshold = now;
			var thresholdTime = now.AddMinutes(-10);
			var initialPeriod = new DateOnlyPeriod(new DateOnly(now.AddDays(-1)), new DateOnly(now.AddDays(_windowSize)));
			var personR1 = Guid.NewGuid();
			var personR2 = Guid.NewGuid();
			var personR3 = Guid.NewGuid();
			var personR4 = Guid.NewGuid();
			var personR5 = Guid.NewGuid();

			addPersonRequestToQueue(monthIs(7), dayIs(15), 22, 0, monthIs(7), dayIs(22), 21, 59, personR1, now.AddMinutes(-11));
			addPersonRequestToQueue(monthIs(7), dayIs(15), 22, 0, monthIs(7), dayIs(16), 21, 59, personR2, now.AddMinutes(-11));
			addPersonRequestToQueue(monthIs(7), dayIs(16), 22, 0, monthIs(7), dayIs(17), 21, 59, personR3, now.AddMinutes(-11));
			addPersonRequestToQueue(monthIs(7), dayIs(17), 22, 0, monthIs(7), dayIs(18), 21, 59, personR4, now.AddMinutes(-11));
			var firstReqThatWillBePickedUp = addPersonRequestToQueue(07, 10, 22, 0, 07, 12, 21, 59, personR5, now.AddMinutes(-11));


			var queuedAbsenceRequests = Target.Get(thresholdTime, pastThreshold, initialPeriod, _windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(1);
			queuedAbsenceRequests.First().ToList().Count.Should().Be.EqualTo(1);
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR5).Should().Be.True();

			removePersonRequestFromQueue(firstReqThatWillBePickedUp);

			addPersonRequestToQueue(07, 10, 22, 0, 07, 12, 21, 59, personR5, now.AddMinutes(-1));

			queuedAbsenceRequests = Target.Get(thresholdTime, pastThreshold, initialPeriod, _windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(1);
			queuedAbsenceRequests.First().ToList().Count.Should().Be.EqualTo(4);
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR1).Should().Be.True();
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR2).Should().Be.True();
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR3).Should().Be.True();
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR4).Should().Be.True();
		}

		[Test]
		public void FetchSingleNearFutureThenThreeBulksForFarFuture()
		{
			var now = new DateTime(2018, 07, 09, 10, 40, 0, DateTimeKind.Utc);
			var pastThreshold = now;
			var thresholdTime = now.AddMinutes(-10);
			var initialPeriod = new DateOnlyPeriod(new DateOnly(now.AddDays(-1)), new DateOnly(now.AddDays(_windowSize)));
			var personR1 = Guid.NewGuid();
			var personR2 = Guid.NewGuid();
			var personR3 = Guid.NewGuid();
			var personR4 = Guid.NewGuid();
			var personR5 = Guid.NewGuid();
			var personR6 = Guid.NewGuid();
			var personR7 = Guid.NewGuid();
			var personR8 = Guid.NewGuid();
			var personR9 = Guid.NewGuid();
			var personR10 = Guid.NewGuid();
			var personR11 = Guid.NewGuid();

			var p1 = addPersonRequestToQueue(monthIs(7), dayIs(10), 10, 0, monthIs(7), dayIs(10), 21, 59, personR1, now.AddMinutes(-11));
			var p2 = addPersonRequestToQueue(monthIs(7), dayIs(10), 22, 0, monthIs(7), dayIs(12), 21, 59, personR2, now.AddMinutes(-11));
			var p3 = addPersonRequestToQueue(monthIs(7), dayIs(10), 22, 0, monthIs(7), dayIs(12), 21, 59, personR3, now.AddMinutes(-1));
			var p4 = addPersonRequestToQueue(monthIs(7), dayIs(10), 10, 0, monthIs(7), dayIs(13), 21, 59, personR4, now.AddMinutes(-1));
			var p5 = addPersonRequestToQueue(monthIs(7), dayIs(13), 11, 0, monthIs(7), dayIs(13), 21, 59, personR5, now.AddMinutes(-11));
			addPersonRequestToQueue(monthIs(7), dayIs(14), 11, 0, monthIs(7), dayIs(16), 21, 59, personR6, now.AddMinutes(-11));
			addPersonRequestToQueue(monthIs(7), dayIs(15), 11, 0, monthIs(7), dayIs(17), 21, 59, personR7, now.AddMinutes(-11));
			addPersonRequestToQueue(monthIs(7), dayIs(15), 11, 0, monthIs(7), dayIs(15), 21, 59, personR8, now.AddMinutes(-1));
			addPersonRequestToQueue(monthIs(7), dayIs(16), 11, 0, monthIs(7), dayIs(16), 21, 59, personR9, now.AddMinutes(-11));
			addPersonRequestToQueue(monthIs(7), dayIs(17), 11, 0, monthIs(7), dayIs(17), 21, 59, personR10, now.AddMinutes(-1));
			addPersonRequestToQueue(monthIs(7), dayIs(18), 11, 0, monthIs(7), dayIs(18), 21, 59, personR11, now.AddMinutes(-11));


			var queuedAbsenceRequests = Target.Get(thresholdTime, pastThreshold, initialPeriod, _windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(1);
			queuedAbsenceRequests.First().ToList().Count.Should().Be.EqualTo(5);
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR1).Should().Be.True();
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR2).Should().Be.True();
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR3).Should().Be.True();
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR4).Should().Be.True();
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR5).Should().Be.True();

			removePersonRequestFromQueue(p1);
			removePersonRequestFromQueue(p2);
			removePersonRequestFromQueue(p3);
			removePersonRequestFromQueue(p4);
			removePersonRequestFromQueue(p5);

			queuedAbsenceRequests = Target.Get(thresholdTime, pastThreshold, initialPeriod, _windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(2);
			queuedAbsenceRequests.First().ToList().Count.Should().Be.EqualTo(5);
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR6).Should().Be.True();
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR7).Should().Be.True();
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR8).Should().Be.True();
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR9).Should().Be.True();
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR10).Should().Be.True();

			queuedAbsenceRequests.Second().ToList().Count.Should().Be.EqualTo(1);
			queuedAbsenceRequests.Second().Any(x => x.PersonRequest == personR11).Should().Be.True();
		}

		[Test]
		public void FlowTestToIgnoreNearFutureIfTooRecent()
		{
			var now = new DateTime(2018, 07, 11, 11, 0, 0, DateTimeKind.Utc);
			var pastThreshold = now;
			var thresholdTime = now.AddMinutes(-10);
			var initialPeriod = new DateOnlyPeriod(new DateOnly(now), new DateOnly(now.AddDays(_windowSize)));
			var personR2 = Guid.NewGuid();
			var personR3 = Guid.NewGuid();
			var personR4 = Guid.NewGuid();
			var personR5 = Guid.NewGuid();
			var personR6 = Guid.NewGuid();
			var personR7 = Guid.NewGuid();

			addPersonRequestToQueue(monthIs(7), dayIs(9), 22, 0, monthIs(7), dayIs(11), 21, 59, personR2, now.AddMinutes(-1));
			var p3 = addPersonRequestToQueue(monthIs(7), dayIs(12), 10, 30, monthIs(7), dayIs(13), 21, 59, personR3, now.AddMinutes(-11));
			var p4 = addPersonRequestToQueue(monthIs(7), dayIs(14), 10, 0, monthIs(7), dayIs(16), 21, 59, personR4, now.AddMinutes(-1));
			var p5 = addPersonRequestToQueue(monthIs(7), dayIs(14), 10, 0, monthIs(7), dayIs(14), 21, 59, personR5, now.AddMinutes(-11));
			var p6 = addPersonRequestToQueue(monthIs(7), dayIs(15), 10, 0, monthIs(7), dayIs(15), 21, 59, personR6, now.AddMinutes(-1));
			var p7 = addPersonRequestToQueue(monthIs(7), dayIs(16), 11, 0, monthIs(7), dayIs(16), 21, 59, personR7, now.AddMinutes(-11));


			var queuedAbsenceRequests = Target.Get(thresholdTime, pastThreshold, initialPeriod, _windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(1);
			queuedAbsenceRequests.First().ToList().Count.Should().Be.EqualTo(1);
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR3).Should().Be.True();

			removePersonRequestFromQueue(p3);

			queuedAbsenceRequests = Target.Get(thresholdTime, pastThreshold, initialPeriod, _windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(1);
			queuedAbsenceRequests.First().ToList().Count.Should().Be.EqualTo(4);
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR4).Should().Be.True();
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR5).Should().Be.True();
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR6).Should().Be.True();
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR7).Should().Be.True();

			removePersonRequestFromQueue(p4);
			removePersonRequestFromQueue(p5);
			removePersonRequestFromQueue(p6);
			removePersonRequestFromQueue(p7);

			queuedAbsenceRequests = Target.Get(thresholdTime, pastThreshold, initialPeriod, _windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(1);
			queuedAbsenceRequests.First().ToList().Count.Should().Be.EqualTo(1);
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR2).Should().Be.True();

		}

		//Fix for the 10 min thingy
		[Test]
		public void FetchBulksFromNearFutureAndFarFarFuture()
		{
			var now = new DateTime(2018, 07, 09, 9, 19, 0, DateTimeKind.Utc);
			var pastThreshold = now;
			var thresholdTime = now.AddMinutes(-10);
			var initialPeriod = new DateOnlyPeriod(new DateOnly(now.AddDays(-1)), new DateOnly(now.AddDays(_windowSize)));
			var personR1 = Guid.NewGuid();
			var personR2 = Guid.NewGuid();
			var personR3 = Guid.NewGuid();
			var personR4 = Guid.NewGuid();
			var personR5 = Guid.NewGuid();

			var p1 = addPersonRequestToQueue(monthIs(7), dayIs(15), 22, 0, monthIs(7), dayIs(22), 21, 59, personR1, now.AddMinutes(-11));
			var p2 = addPersonRequestToQueue(monthIs(7), dayIs(15), 22, 0, monthIs(7), dayIs(16), 21, 59, personR2, now.AddMinutes(-11));
			var p3 = addPersonRequestToQueue(monthIs(7), dayIs(16), 22, 0, monthIs(7), dayIs(17), 21, 59, personR3, now.AddMinutes(-11));
			var p4 = addPersonRequestToQueue(monthIs(7), dayIs(17), 22, 0, monthIs(7), dayIs(18), 21, 59, personR4, now.AddMinutes(-11));
			addPersonRequestToQueue(monthIs(7), dayIs(10), 22, 0, monthIs(7), dayIs(12), 21, 59, personR5, now.AddMinutes(-1));

			var queuedAbsenceRequests = Target.Get(thresholdTime, pastThreshold, initialPeriod, _windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(1);
			queuedAbsenceRequests.First().ToList().Count.Should().Be.EqualTo(4);
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR1).Should().Be.True();
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR2).Should().Be.True();
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR3).Should().Be.True();
			queuedAbsenceRequests.First().Any(x => x.PersonRequest == personR4).Should().Be.True();

			//removing the requests that are are in a bulk
			QueuedAbsenceRequestRepository.Remove(p1);
			QueuedAbsenceRequestRepository.Remove(p2);
			QueuedAbsenceRequestRepository.Remove(p3);
			QueuedAbsenceRequestRepository.Remove(p4);

			queuedAbsenceRequests = Target.Get(thresholdTime, pastThreshold, initialPeriod, _windowSize);
			queuedAbsenceRequests.Count.Should().Be.EqualTo(0);
		}

		private int monthIs(int month)
		{
			return month;
		}

		private int dayIs(int day)
		{
			return day;
		}

		private void removePersonRequestFromQueue(QueuedAbsenceRequest queuedAbsenceRequest)
		{
			QueuedAbsenceRequestRepository.Remove(queuedAbsenceRequest);
		}

		private QueuedAbsenceRequest addPersonRequestToQueue(int startMonth, int startDay, int startHour, int startMin,
			int endMonth, int endDay, int endHour, int endMin, Guid personRequestId, DateTime createdAt, DateTime? sentAt = null)
		{
			DateTime startDateTime = new DateTime(2018, startMonth, startDay, startHour, startMin, 0, DateTimeKind.Utc);
			DateTime endDateTime = new DateTime(2018, endMonth, endDay, endHour, endMin, 0, DateTimeKind.Utc);
			var queuedRequest = new QueuedAbsenceRequest
			{
				StartDateTime = startDateTime,
				EndDateTime = endDateTime,
				Created = createdAt,
				PersonRequest = personRequestId,
				Sent = sentAt
			};
			QueuedAbsenceRequestRepository.Add(queuedRequest);
			return queuedRequest;
		}
	}
}