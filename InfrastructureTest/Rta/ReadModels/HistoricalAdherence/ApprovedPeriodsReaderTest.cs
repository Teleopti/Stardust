using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ApprovePeriodAsInAdherence;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.HistoricalAdherence
{
	[TestFixture]
	[UnitOfWorkTest]
	public class ApprovedPeriodsReaderTest
	{
		public IApprovedPeriodsPersister Target;
		public IApprovedPeriodsReader Reader;

		[Test]
		public void ShouldReadApprovedPeriodForPerson()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			Target.Persist(new ApprovedPeriod
			{
				PersonId = person1,
				StartTime = "2018-01-30 15:00".Utc(),
				EndTime = "2018-01-30 16:00".Utc()
			});

			var result = Reader.Read(person2, "2018-01-30 08:00".Utc(), "2018-01-30 17:00".Utc());

			result.Count().Should().Be(0);
		}

		[Test]
		public void ShouldReadApprovedPeriodTimeAsUtc()
		{
			var person = Guid.NewGuid();
			Target.Persist(new ApprovedPeriod
			{
				PersonId = person,
				StartTime = "2018-01-30 15:00".Utc(),
				EndTime = "2018-01-30 16:00".Utc()
			});

			var result = Reader.Read(person, "2018-01-30 00:00".Utc(), "2018-01-30 23:00".Utc());

			result.Single().StartTime.Kind.Should().Be(DateTimeKind.Utc);
			result.Single().EndTime.Kind.Should().Be(DateTimeKind.Utc);
		}

		[Test]
		public void ShouldExcludeBeforeStartTime()
		{
			var personId = Guid.NewGuid();
			Target.Persist(new ApprovedPeriod
			{
				PersonId = personId,
				StartTime = "2018-01-29 15:00".Utc(),
				EndTime = "2018-01-29 16:00".Utc()
			});
			Target.Persist(new ApprovedPeriod
			{
				PersonId = personId,
				StartTime = "2018-01-30 15:00".Utc(),
				EndTime = "2018-01-30 16:00".Utc()
			});

			var result = Reader.Read(personId, "2018-01-30 08:00".Utc(), "2018-01-30 17:00".Utc());

			result.Single().PersonId.Should().Be(personId);
			result.Single().StartTime.Should().Be("2018-01-30 15:00".Utc());
			result.Single().EndTime.Should().Be("2018-01-30 16:00".Utc());
		}

		[Test]
		public void ShouldExcludeAfterEndTime()
		{
			var personId = Guid.NewGuid();
			Target.Persist(new ApprovedPeriod
			{
				PersonId = personId,
				StartTime = "2018-01-30 18:00".Utc(),
				EndTime = "2018-01-30 19:00".Utc()
			});

			var result = Reader.Read(personId, "2018-01-30 08:00".Utc(), "2018-01-30 17:00".Utc());

			result.Count().Should().Be(0);
		}

		[Test]
		public void ShouldIncludeEndingAfterEndTime()
		{
			var personId = Guid.NewGuid();
			Target.Persist(new ApprovedPeriod
			{
				PersonId = personId,
				StartTime = "2018-01-30 16:00".Utc(),
				EndTime = "2018-01-30 19:00".Utc()
			});

			var result = Reader.Read(personId, "2018-01-30 08:00".Utc(), "2018-01-30 17:00".Utc());

			result.Single().StartTime.Should().Be("2018-01-30 16:00".Utc());
		}

		[Test]
		public void ShouldIncludeStartedBeforeStartTime()
		{
			var personId = Guid.NewGuid();
			Target.Persist(new ApprovedPeriod
			{
				PersonId = personId,
				StartTime = "2018-01-30 07:00".Utc(),
				EndTime = "2018-01-30 09:00".Utc()
			});

			var result = Reader.Read(personId, "2018-01-30 08:00".Utc(), "2018-01-30 17:00".Utc());

			result.Single().StartTime.Should().Be("2018-01-30 07:00".Utc());
		}

		[Test]
		public void ShouldIncludeEndedOnStartTime()
		{
			var personId = Guid.NewGuid();
			Target.Persist(new ApprovedPeriod
			{
				PersonId = personId,
				StartTime = "2018-01-30 07:00".Utc(),
				EndTime = "2018-01-30 08:00".Utc()
			});

			var result = Reader.Read(personId, "2018-01-30 08:00".Utc(), "2018-01-30 17:00".Utc());

			result.Single().StartTime.Should().Be("2018-01-30 07:00".Utc());
		}

		[Test]
		public void ShouldIncludeStartedOnEndTime()
		{
			var personId = Guid.NewGuid();
			Target.Persist(new ApprovedPeriod
			{
				PersonId = personId,
				StartTime = "2018-01-30 17:00".Utc(),
				EndTime = "2018-01-30 18:00".Utc()
			});

			var result = Reader.Read(personId, "2018-01-30 08:00".Utc(), "2018-01-30 17:00".Utc());

			result.Single().StartTime.Should().Be("2018-01-30 17:00".Utc());
		}

		[Test]
		public void ShouldIncludeLoooongPeriod()
		{
			var personId = Guid.NewGuid();
			Target.Persist(new ApprovedPeriod
			{
				PersonId = personId,
				StartTime = "2018-01-30 07:00".Utc(),
				EndTime = "2018-01-30 18:00".Utc()
			});

			var result = Reader.Read(personId, "2018-01-30 08:00".Utc(), "2018-01-30 17:00".Utc());

			result.Single().StartTime.Should().Be("2018-01-30 07:00".Utc());
		}
	}
}