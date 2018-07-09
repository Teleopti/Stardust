using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.Domain.AgentAdherenceDay
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class DurationOfEventsTest
	{
		public IAgentAdherenceDayLoader Target;
		public FakeRtaHistory History;
		public MutableNow Now;
		
		[Test]
		public void ShouldGetNullForOngoing()
		{
			Now.Is("2018-07-09 17:00");
			var personId = Guid.NewGuid();
			History
				.StateChanged(personId, "2018-07-09 10:00")
				;

			var data = Target.Load(personId, "2018-07-09".Date());

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

			var data = Target.Load(personId, "2018-07-09".Date());

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

			var data = Target.Load(personId, "2018-07-09".Date());

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

			var data = Target.Load(personId, "2018-07-09".Date());

			data.Changes().First().Duration.Should().Be("01:00:00");
		}
	}
	
}