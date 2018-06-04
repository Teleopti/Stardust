using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.Domain.AgentAdherenceDay
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
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
				.WithHistoricalStateChange(person, "2016-10-10 08:05", Adherence.Out)
				.WithHistoricalStateChange(person, "2016-10-10 08:15", Adherence.In);

			var data = Target.Load(person, "2016-10-10".Date());

			data.OutOfAdherences().Single().StartTime.Should().Be("2016-10-10 08:05:00".Utc());
			data.OutOfAdherences().Single().EndTime.Should().Be("2016-10-10 08:15:00".Utc());
		}

		[Test]
		public void ShouldNotCreateMultipleOutOfAdherencesWithoutEndTime()
		{
			Now.Is("2016-10-12 12:00");
			var person = Guid.NewGuid();

			Database.WithAgent(person, "nicklas")
				.WithHistoricalStateChange(person, "2016-10-12 08:00", Adherence.Out)
				.WithHistoricalStateChange(person, "2016-10-12 09:00", Adherence.Out);

			var data = Target.Load(person, "2016-10-12".Date());

			data.OutOfAdherences().Single().StartTime.Should().Be("2016-10-12 08:00:00".Utc());
		}

		[Test]
		public void ShouldReadOutOfAdherenceStartedOneDayAgo()
		{
			Now.Is("2016-10-12 12:00");
			var person = Guid.NewGuid();

			Database.WithAgent(person, "nicklas")
				.WithHistoricalStateChange(person, "2016-10-11 09:00", Adherence.Out)
				;

			var data = Target.Load(person, "2016-10-12".Date());

			data.OutOfAdherences().Single().StartTime.Should().Be("2016-10-11 09:00:00".Utc());
		}

		[Test]
		public void ShouldReadOutOfAdherenceStartedOneDayAgo2()
		{
			Now.Is("2016-10-12 12:00");
			var person = Guid.NewGuid();

			Database.WithAgent(person, "nicklas")
				.WithHistoricalStateChange(person, "2016-10-11 09:00", Adherence.Out)
				.WithHistoricalStateChange(person, "2016-10-12 11:00", Adherence.In)
				;

			var data = Target.Load(person, "2016-10-12".Date());

			data.OutOfAdherences().Single().StartTime.Should().Be("2016-10-11 09:00:00".Utc());
			data.OutOfAdherences().Single().EndTime.Should().Be("2016-10-12 11:00:00".Utc());
		}

		[Test]
		public void ShouldCloseOutOfAdherenceOnFirstChange()
		{
			Now.Is("2016-10-12 12:00");
			var person = Guid.NewGuid();

			Database.WithAgent(person, "nicklas")
				.WithHistoricalStateChange(person, "2016-10-11 09:00", Adherence.Out)
				.WithHistoricalStateChange(person, "2016-10-12 10:00", Adherence.Neutral)
				.WithHistoricalStateChange(person, "2016-10-12 11:00", Adherence.In)
				;

			var data = Target.Load(person, "2016-10-12".Date());

			data.OutOfAdherences().Single().StartTime.Should().Be("2016-10-11 09:00:00".Utc());
			data.OutOfAdherences().Single().EndTime.Should().Be("2016-10-12 10:00:00".Utc());
		}

		[Test]
		public void ShouldEndOpenOutOfAdherenceAtCurrentTime()
		{
			Now.Is("2018-02-01 10:00");
			var person = Guid.NewGuid();

			Database.WithAgent(person, "nicklas")
				.WithHistoricalStateChange(person, "2018-02-01 09:00", Adherence.Out)
				;

			var data = Target.Load(person, "2018-02-01".Date());

			data.OutOfAdherences().Single().EndTime.Should().Be("2018-02-01 10:00:00".Utc());
		}
	}
}