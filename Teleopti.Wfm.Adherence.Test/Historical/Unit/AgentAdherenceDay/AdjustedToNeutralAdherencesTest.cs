using System;
using System.Linq;
using NHibernate.Util;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Historical;
using Teleopti.Wfm.Adherence.Historical.AgentAdherenceDay;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.AgentAdherenceDay
{
	[DomainTest]
	public class AdjustedToNeutralAdherencesTest
	{
		public IAgentAdherenceDayLoader Target;
		public FakeRtaHistory History;
		public MutableNow Now;

		[Test]
		public void ShouldGetAdjustedToNeutralAdherencesHistoricalDataForAgent()
		{
			Now.Is("2019-02-25 15:00");
			var person = Guid.NewGuid();
			History
				.AdjustedAdherenceToNeutral("2019-02-25 08:00", "2019-02-25 09:00");
			var data = Target.LoadUntilNow(person);

			data.AdjustedToNeutralAdherences().Single().StartTime.Should().Be("2019-02-25 08:00:00".Utc());
			data.AdjustedToNeutralAdherences().Single().EndTime.Should().Be("2019-02-25 09:00:00".Utc());
		}
		
		[Test]
		public void ShouldGetMultipleAdjustedToNeutralAdherencesHistoricalDataForAgent()
		{
			Now.Is("2019-02-25 15:00");
			var person = Guid.NewGuid();
			History
				.AdjustedAdherenceToNeutral("2019-02-25 08:00", "2019-02-25 09:00")
				.AdjustedAdherenceToNeutral("2019-02-25 09:00", "2019-02-25 10:00");
			var data = Target.LoadUntilNow(person);

			data.AdjustedToNeutralAdherences().First().StartTime.Should().Be("2019-02-25 08:00:00".Utc());
			data.AdjustedToNeutralAdherences().First().EndTime.Should().Be("2019-02-25 09:00:00".Utc());
			data.AdjustedToNeutralAdherences().Last().StartTime.Should().Be("2019-02-25 09:00:00".Utc());
			data.AdjustedToNeutralAdherences().Last().EndTime.Should().Be("2019-02-25 10:00:00".Utc());
		}
		
		[Test]
		public void ShouldNotGetAdjustedToNeutralAdherencesThatDoNotIntersectBuiltDate()
		{
			Now.Is("2019-02-26 15:00");
			var person = Guid.NewGuid();
			History
				.AdjustedAdherenceToNeutral("2019-02-24 08:00", "2019-02-24 09:00")
				.AdjustedAdherenceToNeutral("2019-02-25 09:00", "2019-02-25 10:00");
			var data = Target.LoadUntilNow(person, new DateOnly("2019-02-25".Utc()));

			data.AdjustedToNeutralAdherences().Single().StartTime.Should().Be("2019-02-25 09:00:00".Utc());
			data.AdjustedToNeutralAdherences().Single().EndTime.Should().Be("2019-02-25 10:00:00".Utc());
		}
		
		[Test]
		public void ShouldExcludeAdjustedPeriodFromOutOfAdherences()
		{
			Now.Is("2019-02-28 15:00");
			var person = Guid.NewGuid();
			History
				.StateChanged(person, "2019-02-28 08:00", Adherence.Configuration.Adherence.Out)
				.StateChanged(person, "2019-02-28 09:00", Adherence.Configuration.Adherence.In)
				.AdjustedAdherenceToNeutral("2019-02-28 08:00", "2019-02-28 09:00");
			var data = Target.LoadUntilNow(person);

			data.OutOfAdherences().Should().Be.Empty();
		}
		
		[Test]
		public void ShouldExcludeAdjustedAndApprovedPeriodFromOutOfAdherences()
		{
			Now.Is("2019-02-28 15:00");
			var person = Guid.NewGuid();
			History
				.StateChanged(person, "2019-02-28 08:00", Adherence.Configuration.Adherence.Out)
				.StateChanged(person, "2019-02-28 10:00", Adherence.Configuration.Adherence.In)
				.AdjustedAdherenceToNeutral("2019-02-28 08:00", "2019-02-28 09:00")
				.ApprovedPeriod(person, "2019-02-28 09:00", "2019-02-28 10:00");
			var data = Target.LoadUntilNow(person);

			data.OutOfAdherences().Should().Be.Empty();
		}
		
		[Test]
		public void ShouldIncludeAdjustedPeriodInNeutralAdherences()
		{
			Now.Is("2019-02-28 15:00");
			var person = Guid.NewGuid();
			History
				.StateChanged(person, "2019-02-28 08:00", Adherence.Configuration.Adherence.In)
				.AdjustedAdherenceToNeutral("2019-02-28 08:00", "2019-02-28 09:00");
			var data = Target.LoadUntilNow(person);

			data.NeutralAdherences().Single().StartTime.Should().Be("2019-02-28 08:00".Utc());
			data.NeutralAdherences().Single().EndTime.Should().Be("2019-02-28 09:00".Utc());
		}
		
		[Test]
		public void ShouldExcludeApprovedPeriodFromNeutralAdherences()
		{
			Now.Is("2019-02-28 15:00");
			var person = Guid.NewGuid();
			History
				.StateChanged(person, "2019-02-28 08:00", Adherence.Configuration.Adherence.In)
				.AdjustedAdherenceToNeutral("2019-02-28 08:00", "2019-02-28 09:00")
				.ApprovedPeriod(person, "2019-02-28 08:00", "2019-02-28 09:00");
			var data = Target.LoadUntilNow(person);

			data.NeutralAdherences().Should().Be.Empty();
		}
	}
}