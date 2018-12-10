using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[TestFixture]
	[RtaTest]
	public class StateMappingTest
	{
		public FakeDatabase Database;
		public Rta Target;
		public FakeEventPublisher Publisher;

		[Test]
		public void ShouldMapStateCodeToStateGroup()
		{
			Database
				.WithAgent("usercode")
				.WithStateGroup(null, "state1", true)
				.WithStateCode("statecode1")
				.WithStateGroup("state2")
				.WithStateCode("statecode2")
				;

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode2"
			});

			Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single()
				.StateName.Should().Be("state2");
		}


		[Test]
		public void ShouldMapToLastStateOfSameAgentInBatch()
		{
			Database
				.WithAgent("usercode")
				.WithStateGroup(null, "state1", true)
				.WithStateCode("statecode1")
				.WithStateGroup("state2")
				.WithStateCode("statecode2")
				;

			Target.Process(new BatchForTest
			{
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode",
						StateCode = "statecode1"
					},
					new BatchStateForTest
					{
						UserCode = "usercode",
						StateCode = "statecode2"
					}
				}
			});

			Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single()
				.StateName.Should().Be("state2");
		}
	}
}