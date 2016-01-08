using System;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[Category("LongRunning")]
	[RtaDatabaseTest]
	public class ActualAgentStateReadModelPersistingTest
	{
		public IAgentStateReadModelPersister Target;
		public IAgentStateReadModelReader Reader;

		[Test]
		public void ShouldPersistModel()
		{
			var state = new AgentStateReadModelForTest();

			Target.PersistActualAgentReadModel(state);

			var result = Reader.GetCurrentActualAgentState(state.PersonId);
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistBusinessUnit()
		{
			var businessUnitId = Guid.NewGuid();
			var state = new AgentStateReadModelForTest {BusinessUnitId = businessUnitId};

			Target.PersistActualAgentReadModel(state);

			Reader.GetCurrentActualAgentState(state.PersonId)
				.BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldPersistTeamId()
		{
			var teamId = Guid.NewGuid();
			var state = new AgentStateReadModelForTest {TeamId = teamId};

			Target.PersistActualAgentReadModel(state);

			Reader.GetCurrentActualAgentState(state.PersonId)
				.TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldPersistSiteId()
		{
			var siteId = Guid.NewGuid();
			var state = new AgentStateReadModelForTest {SiteId = siteId};

			Target.PersistActualAgentReadModel(state);

			Reader.GetCurrentActualAgentState(state.PersonId)
				.SiteId.Should().Be(siteId);
		}

		[Test]
		public void ShouldPersistModelWithNullValues()
		{
			var personId = Guid.NewGuid();

			Target.PersistActualAgentReadModel(new AgentStateReadModel
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
				State = null,

				ScheduledId = null,
				Scheduled = null,
				ScheduledNextId = null,
				ScheduledNext = null,
				NextStart = null,

				AlarmId = null,
				AlarmName = null,
				RuleStartTime = null,
				AlarmStartTime = null,
				StaffingEffect = null,
				Adherence = null,
				Color = null,
			});

			var result = Reader.GetCurrentActualAgentState(personId);
			result.Should().Not.Be.Null();
		}


		[Test]
		public void ShouldPersistAlarmStartTime()
		{
			var state = new AgentStateReadModelForTest { AlarmStartTime = "2015-12-11 08:00".Utc()};

			Target.PersistActualAgentReadModel(state);

			Reader.GetCurrentActualAgentState(state.PersonId)
				.AlarmStartTime.Should().Be("2015-12-11 08:00".Utc());
		}

		[Test]
		public void ShouldPersistIsAlarm()
		{
			var state = new AgentStateReadModelForTest {IsRuleAlarm = true};

			Target.PersistActualAgentReadModel(state);

			Reader.GetCurrentActualAgentState(state.PersonId)
				.IsRuleAlarm.Should().Be(true);
		}

		[Test]
		public void ShouldPersistAlarmColor()
		{
			var state = new AgentStateReadModelForTest {AlarmColor = Color.Red.ToArgb()};

			Target.PersistActualAgentReadModel(state);

			Reader.GetCurrentActualAgentState(state.PersonId)
				.AlarmColor.Should().Be(Color.Red.ToArgb());
		}
	}
}