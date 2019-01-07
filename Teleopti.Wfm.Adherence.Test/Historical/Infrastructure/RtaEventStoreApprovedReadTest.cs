using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.Historical.Events;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Historical.Infrastructure
{
	[TestFixture]
	[DatabaseTest]
	public class RtaEventStoreApprovedReadTest
	{
		public IEventPublisher Publisher;
		public IRtaEventStoreReader Target;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldExcludeBeforeStartTime()
		{
			var personId = Guid.NewGuid();
			Publisher.Publish(new PeriodApprovedAsInAdherenceEvent
			{
				PersonId = personId,
				StartTime = "2018-01-29 15:00".Utc(),
				EndTime = "2018-01-29 16:00".Utc()
			});
			Publisher.Publish(new PeriodApprovedAsInAdherenceEvent
			{
				PersonId = personId,
				StartTime = "2018-01-30 15:00".Utc(),
				EndTime = "2018-01-30 16:00".Utc()
			});

			var result = WithUnitOfWork.Get(() =>
				Target.Load(personId, "2018-01-30 08:00".Utc(), "2018-01-30 17:00".Utc())
			).Cast<PeriodApprovedAsInAdherenceEvent>();

			result.Single().PersonId.Should().Be(personId);
			result.Single().StartTime.Should().Be("2018-01-30 15:00".Utc());
			result.Single().EndTime.Should().Be("2018-01-30 16:00".Utc());
		}

		[Test]
		public void ShouldExcludeAfterEndTime()
		{
			var personId = Guid.NewGuid();
			Publisher.Publish(new PeriodApprovedAsInAdherenceEvent
			{
				PersonId = personId,
				StartTime = "2018-01-30 18:00".Utc(),
				EndTime = "2018-01-30 19:00".Utc()
			});

			var result = WithUnitOfWork.Get(() =>
				Target.Load(personId, "2018-01-30 08:00".Utc(), "2018-01-30 17:00".Utc())
			).Cast<PeriodApprovedAsInAdherenceEvent>();

			result.Count().Should().Be(0);
		}

		[Test]
		public void ShouldIncludeEndingAfterEndTime()
		{
			var personId = Guid.NewGuid();
			Publisher.Publish(new PeriodApprovedAsInAdherenceEvent
			{
				PersonId = personId,
				StartTime = "2018-01-30 16:00".Utc(),
				EndTime = "2018-01-30 19:00".Utc()
			});

			var result = WithUnitOfWork.Get(() =>
				Target.Load(personId, "2018-01-30 08:00".Utc(), "2018-01-30 17:00".Utc())
			).Cast<PeriodApprovedAsInAdherenceEvent>();

			result.Single().StartTime.Should().Be("2018-01-30 16:00".Utc());
		}

		[Test]
		public void ShouldIncludeStartedBeforeStartTime()
		{
			var personId = Guid.NewGuid();
			Publisher.Publish(new PeriodApprovedAsInAdherenceEvent
			{
				PersonId = personId,
				StartTime = "2018-01-30 07:00".Utc(),
				EndTime = "2018-01-30 09:00".Utc()
			});

			var result = WithUnitOfWork.Get(() =>
				Target.Load(personId, "2018-01-30 08:00".Utc(), "2018-01-30 17:00".Utc())
			).Cast<PeriodApprovedAsInAdherenceEvent>();

			result.Single().StartTime.Should().Be("2018-01-30 07:00".Utc());
		}

		[Test]
		public void ShouldIncludeEndedOnStartTime()
		{
			var personId = Guid.NewGuid();
			Publisher.Publish(new PeriodApprovedAsInAdherenceEvent
			{
				PersonId = personId,
				StartTime = "2018-01-30 07:00".Utc(),
				EndTime = "2018-01-30 08:00".Utc()
			});

			var result = WithUnitOfWork.Get(() =>
				Target.Load(personId, "2018-01-30 08:00".Utc(), "2018-01-30 17:00".Utc())
			).Cast<PeriodApprovedAsInAdherenceEvent>();

			result.Single().StartTime.Should().Be("2018-01-30 07:00".Utc());
		}


		[Test]
		public void ShouldIncludeStartedOnEndTime()
		{
			var personId = Guid.NewGuid();
			Publisher.Publish(new PeriodApprovedAsInAdherenceEvent
			{
				PersonId = personId,
				StartTime = "2018-01-30 17:00".Utc(),
				EndTime = "2018-01-30 18:00".Utc()
			});

			var result = WithUnitOfWork.Get(() =>
				Target.Load(personId, "2018-01-30 08:00".Utc(), "2018-01-30 17:00".Utc())
			).Cast<PeriodApprovedAsInAdherenceEvent>();

			result.Single().StartTime.Should().Be("2018-01-30 17:00".Utc());
		}

		[Test]
		public void ShouldIncludeLoooongPeriod()
		{
			var personId = Guid.NewGuid();
			Publisher.Publish(new PeriodApprovedAsInAdherenceEvent
			{
				PersonId = personId,
				StartTime = "2018-01-30 07:00".Utc(),
				EndTime = "2018-01-30 18:00".Utc()
			});

			var result = WithUnitOfWork.Get(() =>
				Target.Load(personId, "2018-01-30 08:00".Utc(), "2018-01-30 17:00".Utc())
			).Cast<PeriodApprovedAsInAdherenceEvent>();

			result.Single().StartTime.Should().Be("2018-01-30 07:00".Utc());
		}
	}
}