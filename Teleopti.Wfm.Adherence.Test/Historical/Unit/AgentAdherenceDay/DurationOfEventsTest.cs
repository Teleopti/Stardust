using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.AgentAdherenceDay
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class DurationOfEventsTest
	{
		public IAgentAdherenceDayLoader Target;
		public FakeRtaHistory History;
		public MutableNow Now;
		public FakeDatabase Database;
		
		[Test]
		public void ShouldGetNullForOngoing()
		{
			Now.Is("2018-07-09 17:00");
			var personId = Guid.NewGuid();
			History
				.StateChanged(personId, "2018-07-09 10:00")
				;

			var data = Target.LoadUntilNow(personId, "2018-07-09".Date());

			data.Changes().Single().Duration.Should().Be(null);
		}

		[Test]
		public void ShouldGetDuration()
		{
			Now.Is("2018-07-09 17:00");
			var personId = Guid.NewGuid();
			History
				.StateChanged(personId, "2018-07-09 10:00")
				.StateChanged(personId, "2018-07-09 11:00")
				;

			var data = Target.LoadUntilNow(personId, "2018-07-09".Date());

			data.Changes().First().Duration.Should().Be("01:00:00");
		}
		
		[Test]
		public void ShouldGetDurations()
		{
			Now.Is("2018-07-09 17:00");
			var personId = Guid.NewGuid();
			History
				.StateChanged(personId, "2018-07-09 10:00")
				.StateChanged(personId, "2018-07-09 11:00")
				.StateChanged(personId, "2018-07-09 13:00")
				;

			var data = Target.LoadUntilNow(personId, "2018-07-09".Date());

			data.Changes().First().Duration.Should().Be("01:00:00");
			data.Changes().Second().Duration.Should().Be("02:00:00");
		}
		
				
		[Test]
		public void ShouldHaveDurationOfEventsWithSecondResolution()
		{
			Now.Is("2018-07-09 17:00");
			var personId = Guid.NewGuid();
			History
				.StateChanged(personId, "2018-07-09 10:00")
				.StateChanged(personId, "2018-07-09 11:00:00.888")
				;

			var data = Target.LoadUntilNow(personId, "2018-07-09".Date());

			data.Changes().First().Duration.Should().Be("01:00:00");
		}
		
		
		[Test]
		public void ShouldHaveNullDurationWhenIsSameProof()
		{
			Now.Is("2018-07-19 10:00");
			var personId = Guid.NewGuid();
			History
				.StateChanged(personId, "2018-07-19 10:00")
				.RuleChanged(personId, "2018-07-19 10:00")
				;

			var data = Target.LoadUntilNow(personId, "2018-07-19".Date());

			data.Changes().Single().Duration.Should().Be(null);
		}
	}
	
}