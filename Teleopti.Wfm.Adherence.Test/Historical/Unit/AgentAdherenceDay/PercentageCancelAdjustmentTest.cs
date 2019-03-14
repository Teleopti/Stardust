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
	public class PercentageAdjustToNeutralTest2
	{
		public IAgentAdherenceDayLoader Target;
		public FakeRtaHistory History;
		public MutableNow Now;

		[Test]
		public void ShouldRecalculatePercentage()
		{
			Now.Is("2019-03-15 08:00");
			var person = Guid.NewGuid();
			History
				.ShiftStart(person, "2019-03-14 10:00", "2019-03-14 20:00")
				.StateChanged(person, "2019-03-14 10:00", Adherence.Configuration.Adherence.Out)
				.StateChanged(person, "2019-03-14 15:00", Adherence.Configuration.Adherence.In)
				.AdjustedAdherenceToNeutral("2019-03-14 10:00", "2019-03-14 14:00")
				.CanceledAdjustment("2019-03-14 10:00", "2019-03-14 14:00");

			var result = Target.Load(person, "2019-03-14".Date());

			result.Percentage().Should().Be(50);
		}
	}
}