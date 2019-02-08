using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Historical.AgentAdherenceDay;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.AgentAdherenceDay
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class PercentageAdjustToNeutralTest
	{
		public IAgentAdherenceDayLoader Target;
		public FakeDatabase Database;
		public FakeRtaHistory History;
		public MutableNow Now;

		[Test]
		public void ShouldRecalculatePercentage()
		{
			Now.Is("2019-02-04 08:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2019-02-03")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2019-02-03 08:00", "2019-02-03 16:00");
			History
				.StateChanged(person, "2019-02-03 08:00", Adherence.Configuration.Adherence.Out)
				.StateChanged(person, "2019-02-03 10:00", Adherence.Configuration.Adherence.In)
				.AdjustedAdherenceToNeutral("2019-02-03 12:00", "2019-02-03 16:00");

			var result = Target.LoadUntilNow(person, "2019-02-03".Date());

			result.Percentage().Should().Be(50);
		}

		[Test]
		public void ShouldRecalculatePercentageWhenAdjustingPartlyOverNeutral()
		{
			Now.Is("2019-02-05 08:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2019-02-04")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2019-02-04 08:00", "2019-02-04 20:00");
			History
				.StateChanged(person, "2019-02-04 08:00", Adherence.Configuration.Adherence.Neutral)
				.StateChanged(person, "2019-02-04 10:00", Adherence.Configuration.Adherence.Out)
				.StateChanged(person, "2019-02-04 15:00", Adherence.Configuration.Adherence.In)
				.AdjustedAdherenceToNeutral("2019-02-04 08:00", "2019-02-04 15:00");

			var result = Target.LoadUntilNow(person, "2019-02-04".Date());

			result.Percentage().Should().Be(100);
		}

		[Test]
		public void ShouldRecalculatePercentageWhenAdjustingOverNeutral()
		{
			Now.Is("2019-02-05 08:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2019-02-04")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2019-02-04 08:00", "2019-02-04 20:00");
			History
				.StateChanged(person, "2019-02-04 08:00", Adherence.Configuration.Adherence.Out)
				.StateChanged(person, "2019-02-04 10:00", Adherence.Configuration.Adherence.Neutral)
				.StateChanged(person, "2019-02-04 12:00", Adherence.Configuration.Adherence.In)
				.AdjustedAdherenceToNeutral("2019-02-04 10:00", "2019-02-04 12:00");

			var result = Target.LoadUntilNow(person, "2019-02-04".Date());

			result.Percentage().Should().Be(80);
		}

		[Test]
		public void ShouldRecalculatePercentageWhenAdjustingOverOutAndNeutral()
		{
			Now.Is("2019-02-05 08:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2019-02-04")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2019-02-04 10:00", "2019-02-04 14:00");
			History
				.StateChanged(person, "2019-02-04 10:00", Adherence.Configuration.Adherence.In)
				.StateChanged(person, "2019-02-04 11:00", Adherence.Configuration.Adherence.Out)
				.StateChanged(person, "2019-02-04 13:00", Adherence.Configuration.Adherence.Neutral)
				.AdjustedAdherenceToNeutral("2019-02-04 12:00", "2019-02-04 14:00");

			var result = Target.LoadUntilNow(person, "2019-02-04".Date());

			result.Percentage().Should().Be(50);
		}

		[Test]
		public void ShouldRecalculatePercentageWithExistingNeutral()
		{
			Now.Is("2019-02-05 08:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2019-02-04")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2019-02-04 10:00", "2019-02-04 14:00");
			History
				.StateChanged(person, "2019-02-04 10:00", Adherence.Configuration.Adherence.Neutral)
				.StateChanged(person, "2019-02-04 11:00", Adherence.Configuration.Adherence.Out)
				.StateChanged(person, "2019-02-04 12:00", Adherence.Configuration.Adherence.In)
				.AdjustedAdherenceToNeutral("2019-02-04 13:00", "2019-02-04 14:00");

			var result = Target.LoadUntilNow(person, "2019-02-04".Date());

			result.Percentage().Should().Be(50);
		}
		
		[Test]
		public void ShouldRecalculatePercentageWhenMultipleExistingNeutrals()
		{
			Now.Is("2019-02-05 08:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2019-02-04")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2019-02-04 10:00", "2019-02-04 14:00");
			History
				.StateChanged(person, "2019-02-04 10:00", Adherence.Configuration.Adherence.In)
				.StateChanged(person, "2019-02-04 11:00", Adherence.Configuration.Adherence.Neutral)
				.StateChanged(person, "2019-02-04 12:00", Adherence.Configuration.Adherence.Out)
				.StateChanged(person, "2019-02-04 13:00", Adherence.Configuration.Adherence.Neutral)
				.AdjustedAdherenceToNeutral("2019-02-04 11:00", "2019-02-04 14:00");

			var result = Target.LoadUntilNow(person, "2019-02-04".Date());

			result.Percentage().Should().Be(100);
		}
		
		[Test]
		public void ShouldApplyApprovalLast()
		{
			Now.Is("2019-02-05 08:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2019-02-04")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2019-02-04 10:00", "2019-02-04 14:00");
			History
				.StateChanged(person, "2019-02-04 10:00", Adherence.Configuration.Adherence.In)
				.StateChanged(person, "2019-02-04 11:00", Adherence.Configuration.Adherence.Out)
				.AdjustedAdherenceToNeutral("2019-02-04 11:00", "2019-02-04 12:00")
				.ApprovedPeriod(person, "2019-02-04 11:00", "2019-02-04 12:00");

			var result = Target.LoadUntilNow(person, "2019-02-04".Date());

			result.Percentage().Should().Be(50);
		}
	}
}