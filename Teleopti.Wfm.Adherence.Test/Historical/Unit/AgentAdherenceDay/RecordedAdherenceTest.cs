using System;
using System.Linq;
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
	public class RecordedAdherenceTest
	{
		public IAgentAdherenceDayLoader Target;
		public FakeDatabase Database;
		public FakeRtaHistory History;
		public MutableNow Now;

		[Test]
		public void ShouldCloseOutOfAdherenceOnFirstChange()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				;
			Database.WithHistoricalStateChange("2017-12-08 08:00", Adherence.Configuration.Adherence.Out);
			Database.WithHistoricalStateChange("2017-12-08 10:00", Adherence.Configuration.Adherence.In);
			Database.WithHistoricalStateChange("2017-12-08 12:00", Adherence.Configuration.Adherence.Neutral);

			var result = Target.Load(person);

			result.RecordedOutOfAdherences().First().StartTime.Should().Be("2017-12-08 08:00".Utc());
			result.RecordedOutOfAdherences().First().EndTime.Should().Be("2017-12-08 10:00".Utc());
		}

		[Test]
		public void ShouldCloseOutOfAdherencesFirstChange()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				.WithHistoricalStateChange("2017-12-08 08:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2017-12-08 09:00", Adherence.Configuration.Adherence.In)
				.WithHistoricalStateChange("2017-12-08 10:00", Adherence.Configuration.Adherence.Neutral)
				.WithHistoricalStateChange("2017-12-08 11:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange("2017-12-08 12:00", Adherence.Configuration.Adherence.Neutral)
				.WithHistoricalStateChange("2017-12-08 13:00", Adherence.Configuration.Adherence.In)
				;

			var result = Target.Load(person);

			result.RecordedOutOfAdherences().First().StartTime.Should().Be("2017-12-08 08:00".Utc());
			result.RecordedOutOfAdherences().First().EndTime.Should().Be("2017-12-08 09:00".Utc());
			result.RecordedOutOfAdherences().Second().StartTime.Should().Be("2017-12-08 11:00".Utc());
			result.RecordedOutOfAdherences().Second().EndTime.Should().Be("2017-12-08 12:00".Utc());
		}
		
		[Test]
		public void ShouldExcludeOutOfAdherencePeriodOutsidePeriod()
		{
			Now.Is("2018-11-07 08:00");
			var person = Guid.NewGuid();
			History
				.RuleChanged(person, "2018-11-06 09:00", Adherence.Configuration.Adherence.Out)
				.RuleChanged(person, "2018-11-06 10:00", Adherence.Configuration.Adherence.In)
				.ShiftStart(person, "2018-11-06 12:00", "2018-11-06 17:00")
				;
			
			var result = Target.Load(person, "2018-11-06".Date());
			
			result.RecordedOutOfAdherences().Should().Be.Empty();
		}
	}
}