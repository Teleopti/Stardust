using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[UnitOfWorkTest]
	public class AgentStatePersisterTest
	{
		public IAgentStatePersister Target;

		[Test]
		public void ShouldPersistModel()
		{
			var state = new AgentStateForTest();

			Target.Persist(state);

			var result = Target.Get(state.PersonId);
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistBusinessUnit()
		{
			var businessUnitId = Guid.NewGuid();
			var state = new AgentStateForTest { BusinessUnitId = businessUnitId};

			Target.Persist(state);

			Target.Get(state.PersonId)
				.BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldPersistTeamId()
		{
			var teamId = Guid.NewGuid();
			var state = new AgentStateForTest { TeamId = teamId};

			Target.Persist(state);

			Target.Get(state.PersonId)
				.TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldPersistSiteId()
		{
			var siteId = Guid.NewGuid();
			var state = new AgentStateForTest { SiteId = siteId};

			Target.Persist(state);

			Target.Get(state.PersonId)
				.SiteId.Should().Be(siteId);
		}

		[Test]
		public void ShouldPersistModelWithNullValues()
		{
			var personId = Guid.NewGuid();

			Target.Persist(new AgentStateForTest
			{
				PersonId = personId,
				BusinessUnitId = Guid.NewGuid(),
				TeamId = null,
				SiteId = null,
				PlatformTypeId = Guid.NewGuid(),
				SourceId = null,
				ReceivedTime = "2015-01-02 10:00".Utc(),
				BatchId = null,

				StateCode = null,
				StateGroupId = null,
				StateStartTime = null,

				RuleId = null,
				RuleStartTime = null,
				AlarmStartTime = null,
				StaffingEffect = null,
				Adherence = null
			});

			Target.Get(personId)
				.Should().Not.Be.Null();
		}


		[Test]
		public void ShouldPersistAlarmStartTime()
		{
			var state = new AgentStateForTest { AlarmStartTime = "2015-12-11 08:00".Utc()};

			Target.Persist(state);

			Target.Get(state.PersonId)
				.AlarmStartTime.Should().Be("2015-12-11 08:00".Utc());
		}
		
		[Test]
		public void ShouldDelete()
		{
			var personId = Guid.NewGuid();
			var model = new AgentStateForTest { PersonId = personId };
			Target.Persist(model);

			Target.Delete(personId);

			Target.Get(personId).Should()
				.Be.Null();
		}
	}
}