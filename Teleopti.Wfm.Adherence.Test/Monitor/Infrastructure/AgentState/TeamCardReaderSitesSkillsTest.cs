using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Infrastructure.AgentState
{
	[DatabaseTest]
	[TestFixture]
	public class TeamCardReaderSitesSkillsTest
	{
		public MutableNow Now;
		public Database Database;
		public WithUnitOfWork WithUnitOfWork;
		public IGroupingReadOnlyRepository Groupings;
		public IAgentStateReadModelPersister StatePersister;
		public ITeamCardReader Target;

		[Test]
		public void ShouldRead()
		{
			Now.Is("2016-10-17 08:10");
			Database
				.WithAgent()
				.WithSkill("phone");
			var personId = Database.CurrentPersonId();
			var currentSkillId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { personId });
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = personId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc()
				});
			});

			WithUnitOfWork.Get(() => Target.Read(new[] {currentSkillId})).Single()
				.InAlarmCount.Should().Be(1);
		}

		[Test]
		public void ShouldReadCount()
		{
			Now.Is("2016-10-17 08:10");
			Database
				.WithAgent("personWithoutSkill")
				.WithAgent("personWithSkill")
				.WithSkill("phone")
				;
			var phoneSkill = Database.SkillIdFor("phone");
			var personWithSkill = Database.PersonIdFor("personWithSkill");
			var personWithoutSkill = Database.PersonIdFor("personWithoutSkill");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { personWithSkill, personWithoutSkill });
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = personWithSkill,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc()
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = personWithoutSkill,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc()
				});
			});

			WithUnitOfWork.Get(() => Target.Read(new[] { phoneSkill })).Single()
				.InAlarmCount.Should().Be(1);
		}


	}
}