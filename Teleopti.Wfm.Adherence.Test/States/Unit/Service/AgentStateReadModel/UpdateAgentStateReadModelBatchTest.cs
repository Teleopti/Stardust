using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service.AgentStateReadModel
{
	[RtaTest]
	[TestFixture]
	public class UpdateAgentStateReadModelBatchTest
	{
		public FakeDatabase Database;
		public FakeAgentStateReadModelPersister ReadModels;
		public MutableNow Now;
		public Rta Target;

		[Test]
		public void ShouldUpdateState()
		{
			var users = Enumerable.Range(0, 20).Select(x => $"the-user-{x}").ToArray();
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

			var models = ReadModels.Models.Where(x => (x.FirstName ?? "").StartsWith("the-user")).ToArray();
			models.Should().Have.Count.EqualTo(20);
			models.Select(x => x.StateGroupId).Should().Have.SameValuesAs(state);
		}
	}
}