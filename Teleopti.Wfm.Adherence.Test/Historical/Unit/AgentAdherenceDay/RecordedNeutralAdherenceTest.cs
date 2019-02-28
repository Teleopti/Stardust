using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Historical.AgentAdherenceDay;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.AgentAdherenceDay
{
	[DomainTest]
	public class RecordedNeutralAdherenceTest
	{
		public IAgentAdherenceDayLoader Target;
		public FakeRtaHistory History;
		public MutableNow Now;

		[Test]
		public void ShouldGetRecordedNeutralAdherenceForAgent()
		{
			Now.Is("2019-02-27 15:00");
			var person = Guid.NewGuid();
			History
				.StateChanged(person, "2019-02-27 08:00", Adherence.Configuration.Adherence.Neutral)
				.StateChanged(person, "2019-02-27 09:00", Adherence.Configuration.Adherence.In);
			var data = Target.LoadUntilNow(person);

			data.RecordedNeutralAdherences().Single().StartTime.Should().Be("2019-02-27 08:00:00".Utc());
			data.RecordedNeutralAdherences().Single().EndTime.Should().Be("2019-02-27 09:00:00".Utc());
		}
		
		[Test]
		public void ShouldGetMultipleRecordedNeutralAdherencesForAgent()
		{
			Now.Is("2019-02-27 15:00");
			var person = Guid.NewGuid();
			History
				.StateChanged(person, "2019-02-27 08:00", Adherence.Configuration.Adherence.Neutral)
				.StateChanged(person, "2019-02-27 09:00", Adherence.Configuration.Adherence.In)
				.StateChanged(person, "2019-02-27 10:00", Adherence.Configuration.Adherence.Neutral)
				.StateChanged(person, "2019-02-27 11:00", Adherence.Configuration.Adherence.In);
			var data = Target.LoadUntilNow(person);

			data.RecordedNeutralAdherences().First().StartTime.Should().Be("2019-02-27 08:00:00".Utc());
			data.RecordedNeutralAdherences().First().EndTime.Should().Be("2019-02-27 09:00:00".Utc());
			data.RecordedNeutralAdherences().Last().StartTime.Should().Be("2019-02-27 10:00:00".Utc());
			data.RecordedNeutralAdherences().Last().EndTime.Should().Be("2019-02-27 11:00:00".Utc());
		}
	}
}