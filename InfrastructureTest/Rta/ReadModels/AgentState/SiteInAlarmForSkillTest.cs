using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[DatabaseTest]
	[TestFixture]
	public class SiteInAlarmForSkillTest
	{
		public MutableNow Now;
		public Database Database;
		public WithUnitOfWork WithUnitOfWork;
		public IGroupingReadOnlyRepository Groupings;
		public IAgentStateReadModelPersister StatePersister;
		public ISiteInAlarmReader Target;

		[Test]
		public void ShouldLoad()
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
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = personId,
					SiteId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc()
				});
			});

			WithUnitOfWork.Get(() => Target.ReadForSkills(new[] { currentSkillId }))
				.Count().Should().Be(1);
		}

		[Test]
		public void ShouldReadWithProperties()
		{
			Now.Is("2016-10-17 08:10");
			Database
				.WithAgent()
				.WithSkill("phone");
			var personId = Database.CurrentPersonId();
			var currentSkillId = Database.SkillIdFor("phone");
			var siteId = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { personId });
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = personId,
					SiteId = siteId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc()
				});
			});

			var result = WithUnitOfWork.Get(() => Target.ReadForSkills(new[] { currentSkillId })).Single();

			result.SiteId.Should().Be(siteId);
			result.Count.Should().Be(1);
		}
		
		[Test]
		public void ShouldOnlyLoadForWantedSkill()
		{
			Now.Is("2016-10-17 08:10");
			Database
				.WithAgent("Ashley")
				.WithSkill("Email")
				.WithAgent("Pierre")
				.WithSkill("Phone");
			var siteId = Guid.NewGuid();
			var ashleyId = Database.PersonIdFor("Ashley");
			var pierreId = Database.PersonIdFor("Pierre");
			var phoneSkillId = Database.SkillIdFor("Phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { ashleyId, pierreId });
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = pierreId,
					SiteId = siteId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc()
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = ashleyId,
					SiteId = siteId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc()
				});
			});

			WithUnitOfWork.Get(() => Target.ReadForSkills(new[] { phoneSkillId }))
				.Single().Count.Should().Be(1);
		}
		
		[Test]
		public void ShouldOnlyCountAgentsInAlarm()
		{
			Now.Is("2016-10-17 08:10");
			Database
				.WithAgent("Ashley")
				.WithSkill("Phone")
				.WithAgent("Pierre")
				.WithSkill("Phone");
			var siteId = Guid.NewGuid();
			var ashleyId = Database.PersonIdFor("Ashley");
			var pierreId = Database.PersonIdFor("Pierre");
			var phoneSkillId = Database.SkillIdFor("Phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { ashleyId, pierreId });
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = pierreId,
					SiteId = siteId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc()
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = ashleyId,
					SiteId = siteId,
					IsRuleAlarm = false
				});
			});

			WithUnitOfWork.Get(() => Target.ReadForSkills(new[] { phoneSkillId }))
				.Single().Count.Should().Be(1);
		}


		[Test]
		public void ShouldOnlyCountAgentOnce()
		{
			Now.Is("2016-10-17 08:10");
			Database
				.WithAgent("Ashley")
				.WithSkill("Phone")
				.WithSkill("Email");
			var siteId = Guid.NewGuid();
			var ashleyId = Database.PersonIdFor("Ashley");
			var phoneSkillId = Database.SkillIdFor("Phone");
			var emailSkillId = Database.SkillIdFor("Email");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { ashleyId});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = ashleyId,
					SiteId = siteId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc()
				});
			});

			WithUnitOfWork.Get(() => Target.ReadForSkills(new[] { phoneSkillId, emailSkillId}))
				.Single().Count.Should().Be(1);
		}
		
		[Test]
		public void ShouldNotLoadForPreviousSkill()
		{
			Now.Is("2016-11-21 8:10");
			Database
				.WithPerson("agent1")
				.WithPersonPeriod("2016-01-01".Date())
				.WithSkill("email")
				.WithPersonPeriod("2016-11-20".Date())
				.WithSkill("phone")
				;
			var personId = Database.CurrentPersonId();
			var email = Database.SkillIdFor("email");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { personId });
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = personId,
					SiteId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-11-21 08:00".Utc()
				});
			});

			WithUnitOfWork.Get(() => Target.ReadForSkills(new[] { email }))
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotLoadForFutureSkill()
		{
			Now.Is("2016-11-21 8:10");
			Database
				.WithPerson("agent1")
				.WithPersonPeriod("2016-01-01".Date())
				.WithSkill("email")
				.WithPersonPeriod("2017-01-01".Date())
				.WithSkill("phone")
				;
			var personId = Database.CurrentPersonId();
			var phoneId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { personId });
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = personId,
					SiteId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-11-21 08:00".Utc()
				});
			});

			WithUnitOfWork.Get(() => Target.ReadForSkills(new[] { phoneId }))
				.Should().Be.Empty();
		}


		[Test]
		public void ShouldNotCountDeletedAgents()
		{
			Now.Is("2016-10-17 08:10");
			Database
				.WithAgent("Ashley")
				.WithSkill("Phone")
				.WithAgent("Pierre")
				.WithSkill("Phone");
			var siteId = Guid.NewGuid();
			var ashleyId = Database.PersonIdFor("Ashley");
			var pierreId = Database.PersonIdFor("Pierre");
			var phoneSkillId = Database.SkillIdFor("Phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { ashleyId, pierreId });
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = pierreId,
					SiteId = siteId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc()
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = ashleyId,
					SiteId = siteId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc()
				});
			});
			WithUnitOfWork.Do(() => StatePersister.UpsertDeleted(ashleyId, "2016-10-17 08:30".Utc()));

			WithUnitOfWork.Get(() => Target.ReadForSkills(new[] { phoneSkillId }))
				.Single().Count.Should().Be(1);
		}
	}
}