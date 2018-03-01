using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.Rta.Service.AgentStateReadModel
{
	[RtaTest]
	[TestFixture]
	public class UpdateAgentStateReadModelBatchTest
	{
		public FakeDatabase Database;
		public FakeAgentStateReadModelPersister AgentStates;
		public MutableNow Now;
		public Domain.Rta.Service.Rta Target;

		[Test]
		public void ShouldUpdateState()
		{
			var users = Enumerable.Range(0, 20).Select(x => $"user{x}").ToArray();
			users.ForEach(x => Database.WithAgent(x));
			var state = Guid.NewGuid();
			Database
				.WithStateGroup(state, "state")
				.WithStateCode("state")
				;
			Now.Is("2017-05-15 13:00");

			Target.Process(new BatchForTest
			{
				States = users.Select(x => new BatchStateInputModel
				{
					UserCode = x,
					StateCode = "state"
				}).ToArray()
			});

			AgentStates.Models.Should().Have.Count.EqualTo(20);
			AgentStates.Models.Select(x => x.StateGroupId).Should().Have.SameValuesAs(state);
		}
	}
}
