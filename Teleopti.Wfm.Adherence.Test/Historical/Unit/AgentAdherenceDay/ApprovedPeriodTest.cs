using System;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
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
	public class ApprovedPeriodTest
	{
		public AgentAdherenceDayLoader Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldExcludeApprovedPeriodFromOutOfAdherences()
		{
			Now.Is("2018-02-08 23:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithHistoricalStateChange("2018-02-08 08:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-02-08 09:00", Adherence.Configuration.Adherence.In)
				.WithApprovedPeriod("2018-02-08 08:00", "2018-02-08 09:00")
				;

			var result = Target.LoadUntilNow(person, "2018-02-08".Date());

			result.OutOfAdherences().Should().Be.Empty();
		}

		[Test]
		public void ShouldIncludeApprovedPeriodInRecordedOutOfAdherences()
		{
			Now.Is("2018-02-08 23:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithHistoricalStateChange("2018-02-08 08:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-02-08 09:00", Adherence.Configuration.Adherence.In)
				.WithApprovedPeriod("2018-02-08 08:00", "2018-02-08 09:00")
				;

			var result = Target.LoadUntilNow(person, "2018-02-08".Date());

			result.OutOfAdherences().Should().Be.Empty();
			result.RecordedOutOfAdherences().Single().StartTime.Should().Be("2018-02-08 08:00".Utc());
			result.RecordedOutOfAdherences().Single().EndTime.Should().Be("2018-02-08 09:00".Utc());
		}

		[Test]
		public void ShouldExcludeOneApprovedPeriodFromOutOfAdherences()
		{
			Now.Is("2018-02-08 23:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithHistoricalStateChange("2018-02-08 08:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-02-08 09:00", Adherence.Configuration.Adherence.In)
				.WithHistoricalStateChange("2018-02-08 10:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-02-08 11:00", Adherence.Configuration.Adherence.In)
				.WithApprovedPeriod("2018-02-08 10:00", "2018-02-08 11:00")
				;

			var result = Target.LoadUntilNow(person, "2018-02-08".Date());

			result.OutOfAdherences().Single().StartTime.Should().Be("2018-02-08 08:00".Utc());
			result.OutOfAdherences().Single().EndTime.Should().Be("2018-02-08 09:00".Utc());
			result.RecordedOutOfAdherences().Select(x => x.StartTime).Should().Have.SameValuesAs("2018-02-08 08:00".Utc(), "2018-02-08 10:00".Utc());
			result.RecordedOutOfAdherences().Select(x => x.EndTime).Should().Have.SameValuesAs("2018-02-08 09:00".Utc(), "2018-02-08 11:00".Utc());
		}

		[Test]
		public void ShouldExcludeIntersectingApprovedPeriodAtStartOfOutOfAdherence()
		{
			Now.Is("2018-02-08 23:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithHistoricalStateChange("2018-02-08 08:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-02-08 09:00", Adherence.Configuration.Adherence.In)
				.WithApprovedPeriod("2018-02-08 08:00", "2018-02-08 08:30")
				;

			var result = Target.LoadUntilNow(person, "2018-02-08".Date());

			result.OutOfAdherences().Single().StartTime.Should().Be("2018-02-08 08:30".Utc());
			result.OutOfAdherences().Single().EndTime.Should().Be("2018-02-08 09:00".Utc());
		}

		[Test]
		public void ShouldExcludeIntersectingApprovedPeriodAtEndOfOutOfAdherence()
		{
			Now.Is("2018-02-08 23:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithHistoricalStateChange("2018-02-08 08:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-02-08 09:00", Adherence.Configuration.Adherence.In)
				.WithApprovedPeriod("2018-02-08 08:30", "2018-02-08 09:00")
				;

			var result = Target.LoadUntilNow(person, "2018-02-08".Date());

			result.OutOfAdherences().Single().StartTime.Should().Be("2018-02-08 08:00".Utc());
			result.OutOfAdherences().Single().EndTime.Should().Be("2018-02-08 08:30".Utc());
		}

		[Test]
		public void ShouldExcludeIntersectingApprovedPeriodAtStartAndEndOfOutOfAdherence()
		{
			Now.Is("2018-02-08 23:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithHistoricalStateChange("2018-02-08 08:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-02-08 09:00", Adherence.Configuration.Adherence.In)
				.WithApprovedPeriod("2018-02-08 07:00", "2018-02-08 08:15")
				.WithApprovedPeriod("2018-02-08 08:45", "2018-02-08 10:00")
				;

			var result = Target.LoadUntilNow(person, "2018-02-08".Date());

			Console.WriteLine(JsonConvert.SerializeObject(result.OutOfAdherences()));

			result.OutOfAdherences().Single().StartTime.Should().Be("2018-02-08 08:15".Utc());
			result.OutOfAdherences().Single().EndTime.Should().Be("2018-02-08 08:45".Utc());
		}

		[Test]
		public void ShouldSplitOutOfAdherencePeriodPartlyApproved()
		{
			Now.Is("2018-02-08 23:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithHistoricalStateChange("2018-02-08 08:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-02-08 09:00", Adherence.Configuration.Adherence.In)
				.WithApprovedPeriod("2018-02-08 08:15", "2018-02-08 08:45")
				;

			var result = Target.LoadUntilNow(person, "2018-02-08".Date());

			result.OutOfAdherences().First().StartTime.Should().Be("2018-02-08 08:00".Utc());
			result.OutOfAdherences().First().EndTime.Should().Be("2018-02-08 08:15".Utc());
			result.OutOfAdherences().Second().StartTime.Should().Be("2018-02-08 08:45".Utc());
			result.OutOfAdherences().Second().EndTime.Should().Be("2018-02-08 09:00".Utc());
		}

		[Test]
		public void ShouldSplitOutOfAdherencePeriodPartlyApprovedTwice()
		{
			Now.Is("2018-02-08 23:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithHistoricalStateChange("2018-02-08 08:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-02-08 09:00", Adherence.Configuration.Adherence.In)
				.WithApprovedPeriod("2018-02-08 08:10", "2018-02-08 08:20")
				.WithApprovedPeriod("2018-02-08 08:30", "2018-02-08 08:40")
				;

			var result = Target.LoadUntilNow(person, "2018-02-08".Date());

			result.OutOfAdherences().First().StartTime.Should().Be("2018-02-08 08:00".Utc());
			result.OutOfAdherences().First().EndTime.Should().Be("2018-02-08 08:10".Utc());
			result.OutOfAdherences().Second().StartTime.Should().Be("2018-02-08 08:20".Utc());
			result.OutOfAdherences().Second().EndTime.Should().Be("2018-02-08 08:30".Utc());
			result.OutOfAdherences().Third().StartTime.Should().Be("2018-02-08 08:40".Utc());
			result.OutOfAdherences().Third().EndTime.Should().Be("2018-02-08 09:00".Utc());
		}

		[Test]
		public void ShouldExcludeApprovedPeriodCompletelyOverlappingOutOfAdherence()
		{
			Now.Is("2018-02-08 23:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithHistoricalStateChange("2018-02-08 08:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-02-08 09:00", Adherence.Configuration.Adherence.In)
				.WithApprovedPeriod("2018-02-08 07:00", "2018-02-08 10:00")
				;

			var result = Target.LoadUntilNow(person, "2018-02-08".Date());

			result.OutOfAdherences().Should().Be.Empty();
		}


		[Test]
		public void ShouldExcludeApprovedPeriodsSplittingAndOverlapping()
		{
			Now.Is("2018-02-08 23:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithHistoricalStateChange("2018-02-08 08:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-02-08 09:00", Adherence.Configuration.Adherence.In)
				.WithApprovedPeriod("2018-02-08 07:50", "2018-02-08 08:10")
				.WithApprovedPeriod("2018-02-08 08:20", "2018-02-08 08:30")
				.WithApprovedPeriod("2018-02-08 08:50", "2018-02-08 09:10")
				;

			var result = Target.LoadUntilNow(person, "2018-02-08".Date());

			result.OutOfAdherences().First().StartTime.Should().Be("2018-02-08 08:10".Utc());
			result.OutOfAdherences().First().EndTime.Should().Be("2018-02-08 08:20".Utc());
			result.OutOfAdherences().Second().StartTime.Should().Be("2018-02-08 08:30".Utc());
			result.OutOfAdherences().Second().EndTime.Should().Be("2018-02-08 08:50".Utc());
		}

		[Test]
		public void ShouldExcludeApprovedPeriodFromOutOfAdherenceEndingInTheFuture()
		{
			Now.Is("2018-02-28 09:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithHistoricalStateChange("2018-02-08 08:00", Adherence.Configuration.Adherence.Out)
				.WithApprovedPeriod("2018-02-08 08:00", "2018-02-08 09:00")
				;

			var result = Target.Load(person, "2018-02-08".Date());

			result.OutOfAdherences().Single().StartTime.Should().Be("2018-02-08 09:00".Utc());
			result.OutOfAdherences().Single().EndTime.Should().Be(null);
		}

		[Test]
		public void ShouldExcludeApprovedPeriodFromOutOfAdherenceStartingInThePast()
		{
			Now.Is("2018-02-28 09:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithHistoricalAdherenceDayStart("2018-02-08 00:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2018-02-08 09:00", Adherence.Configuration.Adherence.In)
				.WithApprovedPeriod("2018-02-08 08:00", "2018-02-08 09:00")
				;

			var result = Target.LoadUntilNow(person, "2018-02-08".Date());

			result.OutOfAdherences().Single().StartTime.Should().Be(null);
			result.OutOfAdherences().Single().EndTime.Should().Be("2018-02-08 08:00".Utc());
		}
	}
}