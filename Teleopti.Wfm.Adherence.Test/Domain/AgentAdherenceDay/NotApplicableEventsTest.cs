using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;

namespace Teleopti.Wfm.Adherence.Test.Domain.AgentAdherenceDay
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class NotApplicableEventsTest
	{
		public IAgentAdherenceDayLoader Target;
		public FakeRtaHistory History;
		public MutableNow Now;

		[Test]
		public void ShouldLoadWithEventsThatDoesntApply()
		{
			Now.Is("2018-10-22 15:00");
			var person = Guid.NewGuid();
			History
				.ShiftStart(person, "2018-10-22 09:00", "2018-10-22 17:00")
				.ShiftEnd(person, "2018-10-22 09:00", "2018-10-22 17:00")
				;

			Assert.DoesNotThrow(() =>
			{
				Target.LoadUntilNow(person, "2018-10-22".Date());
			});
		}
	}
}