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
	public class NeutralAdherencesTest
	{
		public IAgentAdherenceDayLoader Target;
		public FakeRtaHistory History;
		public MutableNow Now;

		[Test]
		public void ShouldGetHistoricalDataForAgent()
		{
			Now.Is("2019-02-28 15:00");
			var person = Guid.NewGuid();
			History
				.StateChanged(person, "2019-02-28 08:05", Adherence.Configuration.Adherence.Neutral)
				.StateChanged(person, "2019-02-28 08:15", Adherence.Configuration.Adherence.In);

			var data = Target.LoadUntilNow(person, "2019-02-28".Date());

			data.NeutralAdherences().Single().StartTime.Should().Be("2019-02-28 08:05:00".Utc());
			data.NeutralAdherences().Single().EndTime.Should().Be("2019-02-28 08:15:00".Utc());
		}
		
		[Test]
		public void ShouldGetMultipleNeutralAdherencesHistoricalDataForAgent()
		{
			Now.Is("2019-02-28 15:00");
			var person = Guid.NewGuid();
			History
				.StateChanged(person, "2019-02-28 08:05", Adherence.Configuration.Adherence.Neutral)
				.StateChanged(person, "2019-02-28 08:15", Adherence.Configuration.Adherence.In)
				.StateChanged(person, "2019-02-28 08:30", Adherence.Configuration.Adherence.Neutral)
				.StateChanged(person, "2019-02-28 08:45", Adherence.Configuration.Adherence.In);

			var data = Target.LoadUntilNow(person, "2019-02-28".Date());

			data.NeutralAdherences().First().StartTime.Should().Be("2019-02-28 08:05:00".Utc());
			data.NeutralAdherences().First().EndTime.Should().Be("2019-02-28 08:15:00".Utc());	
			data.NeutralAdherences().Last().StartTime.Should().Be("2019-02-28 08:30:00".Utc());
			data.NeutralAdherences().Last().EndTime.Should().Be("2019-02-28 08:45:00".Utc());
		}
	}
}