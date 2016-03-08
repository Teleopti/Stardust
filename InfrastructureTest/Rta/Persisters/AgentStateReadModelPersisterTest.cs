using System;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[AnalyticsUnitOfWorkTest]
	public class AgentStateReadModelPersisterTest
	{
		public IAgentStateReadModelPersister Target;

		[Test]
		public void ShouldPersistModel()
		{
			var state = new AgentStateReadModelForTest();

			Target.Persist(state);

			var result = Target.Get(state.PersonId);
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistBusinessUnit()
		{
			var businessUnitId = Guid.NewGuid();
			var state = new AgentStateReadModelForTest {BusinessUnitId = businessUnitId};

			Target.Persist(state);

			Target.Get(state.PersonId)
				.BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldPersistTeamId()
		{
			var teamId = Guid.NewGuid();
			var state = new AgentStateReadModelForTest {TeamId = teamId};

			Target.Persist(state);

			Target.Get(state.PersonId)
				.TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldPersistSiteId()
		{
			var siteId = Guid.NewGuid();
			var state = new AgentStateReadModelForTest {SiteId = siteId};

			Target.Persist(state);

			Target.Get(state.PersonId)
				.SiteId.Should().Be(siteId);
		}

		[Test]
		public void ShouldPersistModelWithNullValues()
		{
			var personId = Guid.NewGuid();

			Target.Persist(new AgentStateReadModel
			{
				PersonId = personId,
				BusinessUnitId = Guid.NewGuid(),
				TeamId = null,
				SiteId = null,
				PlatformTypeId = Guid.NewGuid(),
				OriginalDataSourceId = null,
				ReceivedTime = "2015-01-02 10:00".Utc(),
				BatchId = null,

				StateCode = null,
				StateId = null,
				StateStartTime = null,
				StateName = null,

				ScheduledId = null,
				Scheduled = null,
				ScheduledNextId = null,
				ScheduledNext = null,
				NextStart = null,

				RuleId = null,
				RuleName = null,
				RuleStartTime = null,
				AlarmStartTime = null,
				StaffingEffect = null,
				Adherence = null,
				RuleColor = null,
			});

			Target.Get(personId)
				.Should().Not.Be.Null();
		}


		[Test]
		public void ShouldPersistAlarmStartTime()
		{
			var state = new AgentStateReadModelForTest { AlarmStartTime = "2015-12-11 08:00".Utc()};

			Target.Persist(state);

			Target.Get(state.PersonId)
				.AlarmStartTime.Should().Be("2015-12-11 08:00".Utc());
		}

		[Test]
		public void ShouldPersistIsAlarm()
		{
			var state = new AgentStateReadModelForTest {IsAlarm = true};

			Target.Persist(state);

			Target.Get(state.PersonId)
				.IsAlarm.Should().Be(true);
		}

		[Test]
		public void ShouldPersistAlarmColor()
		{
			var state = new AgentStateReadModelForTest {AlarmColor = Color.Red.ToArgb()};

			Target.Persist(state);

			Target.Get(state.PersonId)
				.AlarmColor.Should().Be(Color.Red.ToArgb());
		}

		[Test]
		public void ShouldDelete()
		{
			var personId = Guid.NewGuid();
			var model = new AgentStateReadModelForTest { PersonId = personId };
			Target.Persist(model);

			Target.Delete(personId);

			Target.Get(personId).Should()
				.Be.Null();
		}
	}
}