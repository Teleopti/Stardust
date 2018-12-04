using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Historical.AgentAdherenceDay;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.AgentAdherenceDay
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class ResolutionTest
	{
		public IAgentAdherenceDayLoader Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldExcludeApprovedOutOfAdherence()
		{
			Now.Is("2018-02-20 23:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithHistoricalStateChange("2018-02-20 08:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-02-20 09:00:00.999", Adherence.Configuration.Adherence.In)
				.WithApprovedPeriod("2018-02-20 08:00", "2018-02-20 09:00")
				;

			var result = Target.LoadUntilNow(person, "2018-02-20".Date());

			result.OutOfAdherences().Should().Be.Empty();
		}

		[Test]
		public void ShouldHaveOutOfAdherencesWithSecondResolution()
		{
			Now.Is("2018-02-20 23:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithHistoricalStateChange("2018-02-20 08:00:00.777", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-02-20 09:00:00.888", Adherence.Configuration.Adherence.In)
				;

			var result = Target.LoadUntilNow(person, "2018-02-20".Date());

			result.OutOfAdherences().Single().StartTime.Should().Be("2018-02-20 08:00:00".Utc());
			result.OutOfAdherences().Single().EndTime.Should().Be("2018-02-20 09:00:00".Utc());
		}

		[Test]
		public void ShouldHaveOutOfAdherenceUntilNowWithSecondResolution()
		{
			Now.Is("2018-02-20 08:12:34.567");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithHistoricalStateChange("2018-02-20 08:00", Adherence.Configuration.Adherence.Out)
				;

			var result = Target.LoadUntilNow(person, "2018-02-20".Date());

			result.OutOfAdherences().Single().StartTime.Should().Be("2018-02-20 08:00:00".Utc());
			result.OutOfAdherences().Single().EndTime.Should().Be("2018-02-20 08:12:34".Utc());
		}

		[Test]
		public void ShouldCalculatePercentageWithSecondResolution()
		{
			Now.Is("2018-02-20 23:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2018-02-20")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2018-02-20 08:00", "2018-02-20 17:00:00")
				.WithHistoricalStateChange("2018-02-20 08:00:00.999", Adherence.Configuration.Adherence.Neutral)
				.WithHistoricalStateChange("2018-02-20 16:59:50", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-02-20 16:59:55.001", Adherence.Configuration.Adherence.In)
				.WithHistoricalStateChange("2018-02-20 16:59:56.999", Adherence.Configuration.Adherence.Out)
				;

			var result = Target.LoadUntilNow(person, "2018-02-20".Date());

			result.Percentage().Should().Be(10);
		}

		[Test]
		public void ShouldCalculatePercentageWithSecondResolutionForNow()
		{
			Now.Is("2018-02-20 16:59:59.999");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2018-02-20")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2018-02-20 08:00", "2018-02-20 17:00:00")
				.WithHistoricalStateChange("2018-02-20 08:00", Adherence.Configuration.Adherence.Neutral)
				.WithHistoricalStateChange("2018-02-20 16:59:55", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-02-20 16:59:56", Adherence.Configuration.Adherence.In)
				.WithHistoricalStateChange("2018-02-20 16:59:57", Adherence.Configuration.Adherence.Out)
				;

			var result = Target.LoadUntilNow(person, "2018-02-20".Date());

			result.Percentage().Should().Be(25);
		}

		[Test]
		public void ShouldHaveRecordedOutOfAdherencesWithSecondResolution()
		{
			Now.Is("2018-02-20 23:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithHistoricalStateChange("2018-02-20 08:00:00.777", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-02-20 09:00:00.888", Adherence.Configuration.Adherence.In)
				;

			var result = Target.LoadUntilNow(person, "2018-02-20".Date());

			result.RecordedOutOfAdherences().Single().StartTime.Should().Be("2018-02-20 08:00:00".Utc());
			result.RecordedOutOfAdherences().Single().EndTime.Should().Be("2018-02-20 09:00:00".Utc());
		}

		[Test]
		public void ShouldNotHaveOutOfAdherenceShorterThanOneSecond()
		{
			Now.Is("2018-02-20 23:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithHistoricalStateChange("2018-02-20 08:00:00.000", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-02-20 08:00:00.100", Adherence.Configuration.Adherence.In)
				.WithHistoricalStateChange("2018-02-20 08:01:00.000", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-02-20 08:01:00.100", Adherence.Configuration.Adherence.In)
				;

			var result = Target.LoadUntilNow(person, "2018-02-20".Date());

			result.OutOfAdherences().Should().Be.Empty();
		}

		[Test]
		public void ShouldNotHaveOutOfAdherenceUntilNowShorterThanOneSecond()
		{
			Now.Is("2018-02-20 08:00:00.100");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithHistoricalStateChange("2018-02-20 08:00:00", Adherence.Configuration.Adherence.Out)
				;

			var result = Target.LoadUntilNow(person, "2018-02-20".Date());

			result.OutOfAdherences().Should().Be.Empty();
		}

		[Test]
		public void ShouldNotHaveOutOfAdherencesWithNanoSecondResolution()
		{
			Now.Is("2018-03-26 23:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithHistoricalStateChange("2018-03-26 08:00:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-03-26 09:00:00.2706", Adherence.Configuration.Adherence.In)
				.WithApprovedPeriod(person, "2018-03-26 08:00:00", "2018-03-26 09:00:00")
				;

			var result = Target.LoadUntilNow(person, "2018-03-26".Date());

			result.OutOfAdherences().Should().Be.Empty();
		}

		[Test]
		public void ShouldSplitOutOfAdherencePeriodBecauseOfTinyNeutralPeriod()
		{
			Now.Is("2018-03-26 23:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithHistoricalStateChange("2018-03-26 08:00:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-03-26 08:30:00.100", Adherence.Configuration.Adherence.Neutral)
				.WithHistoricalStateChange("2018-03-26 08:30:00.200", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-03-26 09:00:00", Adherence.Configuration.Adherence.In)
				;

			var result = Target.LoadUntilNow(person, "2018-03-26".Date());

			result.OutOfAdherences().Single().StartTime.Should().Be("2018-03-26 08:00:00".Utc());
			result.OutOfAdherences().Single().EndTime.Should().Be("2018-03-26 09:00:00".Utc());
		}
	}
}