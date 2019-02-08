using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Historical.AgentAdherenceDay;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.AgentAdherenceDay
{
	[DomainTest]
	public class AdherencesTest
	{
		public IAgentAdherenceDayLoader Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldGetHistoricalDataForAgent()
		{
			Now.Is("2016-10-10 15:00");
			var person = Guid.NewGuid();
			Database
				.WithHistoricalStateChange(person, "2016-10-10 08:05", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange(person, "2016-10-10 08:15", Adherence.Configuration.Adherence.In);

			var data = Target.LoadUntilNow(person, "2016-10-10".Date());

			data.OutOfAdherences().Single().StartTime.Should().Be("2016-10-10 08:05:00".Utc());
			data.OutOfAdherences().Single().EndTime.Should().Be("2016-10-10 08:15:00".Utc());
		}

		[Test]
		public void ShouldNotCreateMultipleOutOfAdherencesWithoutEndTime()
		{
			Now.Is("2016-10-12 12:00");
			var person = Guid.NewGuid();

			Database.WithAgent(person, "nicklas")
				.WithHistoricalStateChange(person, "2016-10-12 08:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange(person, "2016-10-12 09:00", Adherence.Configuration.Adherence.Out);

			var data = Target.LoadUntilNow(person, "2016-10-12".Date());

			data.OutOfAdherences().Single().StartTime.Should().Be("2016-10-12 08:00:00".Utc());
		}

		[Test]
		public void ShouldReadOutOfAdherenceStartedOneDayAgo()
		{
			Now.Is("2016-10-12 12:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithHistoricalStateChange(person, "2016-10-11 09:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalAdherenceDayStart("2016-10-12 00:00", Adherence.Configuration.Adherence.Out)
				;

			var data = Target.LoadUntilNow(person, "2016-10-12".Date());

			data.OutOfAdherences().Single().StartTime.Should().Be(null);
		}

		[Test]
		public void ShouldReadOutOfAdherenceStartedOneDayAgo2()
		{
			Now.Is("2016-10-12 12:00");
			var person = Guid.NewGuid();

			Database
				.WithAgent(person)
				.WithHistoricalStateChange(person, "2016-10-11 09:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalAdherenceDayStart(person, "2016-10-12 00:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange(person, "2016-10-12 11:00", Adherence.Configuration.Adherence.In)
				;

			var data = Target.LoadUntilNow(person, "2016-10-12".Date());

			data.OutOfAdherences().Single().StartTime.Should().Be(null);
			data.OutOfAdherences().Single().EndTime.Should().Be("2016-10-12 11:00:00".Utc());
		}

		[Test]
		public void ShouldReadOutOfAdherenceStartedTwoDaysAgo()
		{
			Now.Is("2016-10-12 12:00");
			var person = Guid.NewGuid();

			Database.WithAgent(person)
				.WithHistoricalStateChange(person, "2016-10-10 09:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange(person, "2016-10-11 09:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalAdherenceDayStart(person, "2016-10-12 00:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange(person, "2016-10-12 11:00", Adherence.Configuration.Adherence.In)
				;

			var data = Target.LoadUntilNow(person, "2016-10-12".Date());

			data.OutOfAdherences().Single().StartTime.Should().Be(null);
			data.OutOfAdherences().Single().EndTime.Should().Be("2016-10-12 11:00:00".Utc());
		}

		[Test]
		public void ShouldCloseOutOfAdherenceOnFirstChange()
		{
			Now.Is("2016-10-12 12:00");
			var person = Guid.NewGuid();
			Database.WithAgent(person)
				.WithHistoricalStateChange(person, "2016-10-12 00:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange(person, "2016-10-12 10:00", Adherence.Configuration.Adherence.Neutral)
				.WithHistoricalStateChange(person, "2016-10-12 11:00", Adherence.Configuration.Adherence.In)
				;

			var data = Target.LoadUntilNow(person, "2016-10-12".Date());

			data.OutOfAdherences().Single().StartTime.Should().Be(null);
			data.OutOfAdherences().Single().EndTime.Should().Be("2016-10-12 10:00:00".Utc());
		}

		[Test]
		public void ShouldEndOpenOutOfAdherenceAtCurrentTime()
		{
			Now.Is("2018-02-01 10:00");
			var person = Guid.NewGuid();
			Database.WithAgent(person)
				.WithHistoricalStateChange(person, "2018-02-01 09:00", Adherence.Configuration.Adherence.Out)
				;

			var data = Target.LoadUntilNow(person, "2018-02-01".Date());

			data.OutOfAdherences().Single().EndTime.Should().Be("2018-02-01 10:00:00".Utc());
		}

		[Test]
		public void ShouldNotEndOpenOutOfAdherenceIfUnknown()
		{
			Now.Is("2018-02-03 09:00");
			var person = Guid.NewGuid();
			Database.WithAgent(person)
				.WithHistoricalStateChange(person, "2018-02-01 09:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange(person, "2018-02-02 09:00", Adherence.Configuration.Adherence.In)
				;

			var data = Target.LoadUntilNow(person, "2018-02-01".Date());

			data.OutOfAdherences().Single().EndTime.Should().Be(null);
		}
	}
}