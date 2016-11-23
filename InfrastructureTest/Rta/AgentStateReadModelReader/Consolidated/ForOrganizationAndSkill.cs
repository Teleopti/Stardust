using System;
using System.Linq;
using NHibernate.Util;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta.AgentStateReadModelReader.Consolidated
{
	[DatabaseTest]
	[TestFixture]
	[Toggle(Toggles.RTA_QuicklyChangeAgentsSelection_40610)]
	public class ForOrganizationAndSkill
	{
		public IGroupingReadOnlyRepository Groupings;
		public Database Database;
		public IAgentStateReadModelPersister StatePersister;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IAgentStateReadModelReader Target;

		[Test]
		public void ShouldLoadForSiteAndSkill()
		{
			Now.Is("2016-11-07 08:00");
			Database
				.WithAgent("wrongSite")
				.WithSkill("phone")
				.WithSite()
				.WithAgent("expected")
				.WithSkill("phone")
				.WithAgent("wrongSkill")
				.WithSkill("email")
				;
			var expected = Database.PersonIdFor("expected");
			var wrongSkill = Database.PersonIdFor("wrongSkill");
			var wrongSite = Database.PersonIdFor("wrongSite");
			var siteId = Database.CurrentSiteId();
			var currentSkillId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {expected, wrongSkill, wrongSite});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = expected,
					SiteId = siteId
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = wrongSkill,
					SiteId = siteId
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = wrongSite,
					SiteId = Guid.NewGuid()
				});
			});

			WithUnitOfWork.Get(() => Target.ReadFor(new[] {siteId}, null, new[] {currentSkillId}))
				.Single().PersonId.Should().Be(expected);
		}

		[Test]
		public void ShouldNotLoadDuplicateRowsForSites()
		{
			Now.Is("2016-11-07 08:00");
			Database
				.WithAgent("expected")
				.WithSkill("phone")
				.WithSkill("email")
				;
			var expected = Database.PersonIdFor("expected");
			var siteId = Database.CurrentSiteId();
			var phone = Database.SkillIdFor("phone");
			var email = Database.SkillIdFor("email");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {expected});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = expected,
					SiteId = siteId
				});
			});

			WithUnitOfWork.Get(() => Target.ReadFor(new[] {siteId}, null, new[] {phone, email}))
				.Single().PersonId.Should().Be(expected);
		}

		[Test]
		public void ShouldLoadForTeamAndSkill()
		{
			Now.Is("2016-11-07 08:00");
			Database
				.WithAgent("wrongSite")
				.WithSkill("phone")
				.WithTeam()
				.WithAgent("expected")
				.WithSkill("phone")
				.WithAgent("wrongSkill")
				.WithSkill("email")
				;
			var expected = Database.PersonIdFor("expected");
			var wrongSkill = Database.PersonIdFor("wrongSkill");
			var wrongTeam = Database.PersonIdFor("wrongSite");
			var teamId = Database.CurrentTeamId();
			var currentSkillId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {expected, wrongSkill, wrongTeam});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = expected,
					TeamId = teamId
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = wrongSkill,
					TeamId = teamId
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = wrongTeam,
					TeamId = Guid.NewGuid()
				});
			});

			WithUnitOfWork.Get(() => Target.ReadFor(null, new[] {teamId}, new[] {currentSkillId}))
				.Single().PersonId.Should().Be(expected);
		}

		[Test]
		public void ShouldNotLoadDuplicateRowsForTeams()
		{
			Now.Is("2016-11-07 08:00");
			Database
				.WithAgent("expected")
				.WithSkill("phone")
				.WithSkill("email")
				;
			var expected = Database.PersonIdFor("expected");
			var teamId = Database.CurrentTeamId();
			var phone = Database.SkillIdFor("phone");
			var email = Database.SkillIdFor("email");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {expected});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = expected,
					TeamId = teamId
				});
			});

			WithUnitOfWork.Get(() => Target.ReadFor(null, new[] {teamId}, new[] {phone, email}))
				.Single().PersonId.Should().Be(expected);
		}


		//[Test]
		//public void ShouldLoadInAlarmForSiteAndSkill()
		//{
		//	Now.Is("2016-11-07 08:10");
		//	Database
		//		.WithAgent("wrongSite")
		//		.WithSkill("phone")
		//		.WithSite()
		//		.WithAgent("wrongSkill")
		//		.WithSkill("email")
		//		.WithAgent("notInAlarm")
		//		.WithSkill("phone")
		//		.WithAgent("expected")
		//		.WithSkill("phone")
		//		;
		//	var wrongSite = Database.PersonIdFor("wrongSite");
		//	var wrongSkill = Database.PersonIdFor("wrongSkill");
		//	var notInAlarm = Database.PersonIdFor("notInAlarm");
		//	var expected = Database.PersonIdFor("expected");
		//	var site = Database.CurrentSiteId();
		//	var currentSkillId = Database.SkillIdFor("phone");
		//	WithUnitOfWork.Do(() =>
		//	{
		//		Groupings.UpdateGroupingReadModel(new[] {expected, wrongSite, wrongSkill, notInAlarm});
		//		StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
		//		{
		//			PersonId = expected,
		//			SiteId = site,
		//			AlarmStartTime = "2016-11-07 08:00".Utc(),
		//			IsRuleAlarm = true
		//		});
		//		StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
		//		{
		//			PersonId = wrongSite,
		//			SiteId = Guid.NewGuid(),
		//			AlarmStartTime = "2016-11-07 08:00".Utc(),
		//			IsRuleAlarm = true
		//		});
		//		StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
		//		{
		//			PersonId = wrongSkill,
		//			SiteId = site,
		//			AlarmStartTime = "2016-11-07 08:00".Utc(),
		//			IsRuleAlarm = true
		//		});
		//		StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
		//		{
		//			PersonId = notInAlarm,
		//			SiteId = site
		//		});
		//	});

		//	WithUnitOfWork.Get(() => Target.ReadInAlarmsForSitesAndSkills(new[] {site}, new[] {currentSkillId}))
		//		.Single().PersonId.Should().Be(expected);
		//}


		//[Test]
		//public void ShouldLoadTop50InAlarmForSiteAndSkill()
		//{
		//	Now.Is("2016-11-07 08:10");
		//	Database
		//		.WithSite();
		//	var site = Database.CurrentSiteId();
		//	Enumerable.Range(1, 51)
		//		.ForEach(i =>
		//		{
		//			Database
		//				.WithAgent(i.ToString())
		//				.WithSkill("phone")
		//				;
		//			var current = Database.PersonIdFor(i.ToString());
		//			WithUnitOfWork.Do(() =>
		//			{
		//				Groupings.UpdateGroupingReadModel(new[] {current});
		//				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
		//				{
		//					PersonId = current,
		//					SiteId = site,
		//					AlarmStartTime = "2016-11-07 08:00".Utc(),
		//					IsRuleAlarm = true
		//				});
		//			});
		//		});
		//	var skillId = Database.SkillIdFor("phone");

		//	WithUnitOfWork.Get(() => Target.ReadInAlarmsForSitesAndSkills(new[] {site}, new[] {skillId}))
		//		.Select(x => x.PersonId).Distinct()
		//		.Should().Have.Count.EqualTo(50);
		//}


		//[Test]
		//public void ShouldLoadInAlarmForTeamAndSkill()
		//{
		//	Now.Is("2016-11-07 08:10");
		//	Database
		//		.WithAgent("wrongTeam")
		//		.WithSkill("phone")
		//		.WithTeam()
		//		.WithAgent("wrongSkill")
		//		.WithSkill("email")
		//		.WithAgent("notInAlarm")
		//		.WithSkill("phone")
		//		.WithAgent("expected")
		//		.WithSkill("phone")
		//		;
		//	var wrongTeam = Database.PersonIdFor("wrongTeam");
		//	var wrongSkill = Database.PersonIdFor("wrongSkill");
		//	var notInAlarm = Database.PersonIdFor("notInAlarm");
		//	var expected = Database.PersonIdFor("expected");
		//	var team = Database.CurrentTeamId();
		//	var currentSkillId = Database.SkillIdFor("phone");
		//	WithUnitOfWork.Do(() =>
		//	{
		//		Groupings.UpdateGroupingReadModel(new[] {expected, wrongTeam, wrongSkill, notInAlarm});
		//		StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
		//		{
		//			PersonId = expected,
		//			TeamId = team,
		//			AlarmStartTime = "2016-11-07 08:00".Utc(),
		//			IsRuleAlarm = true
		//		});
		//		StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
		//		{
		//			PersonId = wrongTeam,
		//			TeamId = Guid.NewGuid(),
		//			AlarmStartTime = "2016-11-07 08:00".Utc(),
		//			IsRuleAlarm = true
		//		});
		//		StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
		//		{
		//			PersonId = wrongSkill,
		//			TeamId = team,
		//			AlarmStartTime = "2016-11-07 08:00".Utc(),
		//			IsRuleAlarm = true
		//		});
		//		StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
		//		{
		//			PersonId = notInAlarm,
		//			TeamId = team,
		//		});
		//	});

		//	WithUnitOfWork.Get(() => Target.ReadInAlarmsForTeamsAndSkills(new[] {team}, new[] {currentSkillId}))
		//		.Single().PersonId.Should().Be(expected);
		//}


		//[Test]
		//public void ShouldLoadTop50InAlarmForTeamAndSkill()
		//{
		//	Now.Is("2016-11-07 08:10");
		//	Database
		//		.WithTeam();
		//	var teamId = Database.CurrentTeamId();
		//	Enumerable.Range(1, 51)
		//		.ForEach(i =>
		//		{
		//			Database
		//				.WithAgent(i.ToString())
		//				.WithSkill("phone")
		//				;
		//			var current = Database.PersonIdFor(i.ToString());
		//			WithUnitOfWork.Do(() =>
		//			{
		//				Groupings.UpdateGroupingReadModel(new[] {current});
		//				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
		//				{
		//					PersonId = current,
		//					TeamId = teamId,
		//					AlarmStartTime = "2016-11-07 08:00".Utc(),
		//					IsRuleAlarm = true
		//				});
		//			});
		//		});
		//	var skillId = Database.SkillIdFor("phone");

		//	WithUnitOfWork.Get(() => Target.ReadInAlarmsForTeamsAndSkills(new[] {teamId}, new[] {skillId}))
		//		.Select(x => x.PersonId).Distinct()
		//		.Should().Have.Count.EqualTo(50);
		//}


		//[Test]
		//public void ShouldNotLoadDuplicateRowsForTeamsAndSkill()
		//{
		//	Now.Is("2016-11-07 08:10");
		//	Database
		//		.WithAgent("expected")
		//		.WithSkill("phone")
		//		.WithSkill("email")
		//		;
		//	var expected = Database.PersonIdFor("expected");
		//	var teamId = Database.CurrentTeamId();
		//	var phone = Database.SkillIdFor("phone");
		//	var email = Database.SkillIdFor("email");
		//	WithUnitOfWork.Do(() =>
		//	{
		//		Groupings.UpdateGroupingReadModel(new[] {expected});
		//		StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
		//		{
		//			PersonId = expected,
		//			TeamId = teamId,
		//			AlarmStartTime = "2016-11-07 08:00".Utc(),
		//			IsRuleAlarm = true
		//		});
		//	});

		//	WithUnitOfWork.Get(() => Target.ReadInAlarmsForTeamsAndSkills(new[] {teamId}, new[] {phone, email}))
		//		.Single().PersonId.Should().Be(expected);
		//}
		
		//[Test]
		//public void ShouldLoadInAlarmForMultipleTeamAndSkill()
		//{
		//	Now.Is("2016-11-07 08:10");
		//	Database
		//		.WithTeam("A")
		//		.WithAgent("person1")
		//		.WithSkill("phone")
		//		.WithTeam("B")
		//		.WithAgent("person2")
		//		.WithSkill("phone")
		//		;
		//	var person1 = Database.PersonIdFor("person1");
		//	var person2 = Database.PersonIdFor("person2");
		//	var teamA = Database.TeamIdFor("A");
		//	var teamB = Database.TeamIdFor("B");
		//	var currentSkillId = Database.SkillIdFor("phone");
		//	WithUnitOfWork.Do(() =>
		//	{
		//		Groupings.UpdateGroupingReadModel(new[] { person1, person2 });
		//		StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
		//		{
		//			PersonId = person1,
		//			TeamId = teamA,
		//			AlarmStartTime = "2016-11-07 08:00".Utc(),
		//			IsRuleAlarm = true
		//		});
		//		StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
		//		{
		//			PersonId = person2,
		//			TeamId = teamB,
		//			AlarmStartTime = "2016-11-07 08:00".Utc(),
		//			IsRuleAlarm = true
		//		});
		//	});

		//	WithUnitOfWork.Get(() => Target.ReadInAlarmsForTeamsAndSkills(new[] {teamA, teamB}, new[] {currentSkillId}))
		//		.Should().Have.Count.EqualTo(2);
		//}
	}
}