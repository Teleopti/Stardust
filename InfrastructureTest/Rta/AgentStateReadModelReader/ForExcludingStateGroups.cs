using System;
using System.Linq;
using NHibernate.Util;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.AgentStateReadModelReader
{
	[DatabaseTest]
	[TestFixture]
	public class ForExcludingStateGroupsForSiteAndSkill
	{
		public IGroupingReadOnlyRepository Groupings;
		public Database Database;
		public IAgentStateReadModelPersister StatePersister;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IAgentStateReadModelLegacyReader Target;

		[Test]
		public void ShouldLoadForSitesAndSkill()
		{
			Now.Is("2016-11-07 08:10");
			Database
				.WithSite()
				.WithAgent("expected")
				.WithSkill("phone")
				;

			var siteId = Database.CurrentSiteId();
			var expected = Database.PersonIdFor("expected");
			var skillId = Database.SkillIdFor("phone");
			var loggedOut = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {expected});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = expected,
					SiteId = siteId,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = Guid.NewGuid()
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = Guid.NewGuid(),
					SiteId = siteId,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = loggedOut
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = Guid.NewGuid(),
					SiteId = siteId,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = false,
					StateGroupId = Guid.NewGuid()
				});
			});

			WithUnitOfWork.Get(() => Target.ReadInAlarmExcludingPhoneStatesForSitesAndSkill(new[] { siteId }, new[] { skillId }, new Guid?[] { loggedOut }))
				.Single().PersonId.Should().Be(expected);
		}

		[Test]
		public void ShouldLoadTop50InAlarmForSiteAndSkill()
		{
			Now.Is("2016-11-07 08:10");
			Database
				.WithSite();
			var site = Database.CurrentSiteId();
			Enumerable.Range(1, 51)
				.ForEach(i =>
				{
					Database
						.WithAgent(i.ToString())
						.WithSkill("phone")
						;
					var current = Database.PersonIdFor(i.ToString());
					WithUnitOfWork.Do(() =>
					{
						Groupings.UpdateGroupingReadModel(new[] { current });
						StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
						{
							PersonId = current,
							SiteId = site,
							AlarmStartTime = "2016-11-07 08:00".Utc(),
							IsRuleAlarm = true,
							StateGroupId = Guid.NewGuid()
						});
					});
				});
			var skillId = Database.SkillIdFor("phone");

			WithUnitOfWork.Get(() => Target.ReadInAlarmExcludingPhoneStatesForSitesAndSkill(new[] { site }, new[] { skillId }, new Guid?[] {null}))
				.Select(x => x.PersonId).Distinct()
				.Should().Have.Count.EqualTo(50);
		}

		[Test]
		public void ShouldNotLoadDuplicateRowsForSitesAndSkill()
		{
			Now.Is("2016-11-07 08:10");
			Database
				.WithAgent("expected")
				.WithSkill("phone")
				.WithSkill("email")
				;
			var expected = Database.PersonIdFor("expected");
			var siteId = Database.CurrentSiteId();
			var phone = Database.SkillIdFor("phone");
			var email = Database.SkillIdFor("email");
			var loggedOut = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { expected });
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = expected,
					SiteId = siteId,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					StateGroupId = Guid.NewGuid()
				});
			});

			WithUnitOfWork.Get(() => Target.ReadInAlarmExcludingPhoneStatesForSitesAndSkill(new[] { siteId }, new[] { phone, email }, new Guid?[] { loggedOut }))
				.Single().PersonId.Should().Be(expected);
		}

		[Test]
		public void ShouldLoadInAlarmForSiteAndSkill()
		{
			Now.Is("2016-11-07 08:10");
			Database
				.WithAgent("wrongSite")
				.WithSkill("phone")
				.WithSite()
				.WithAgent("wrongSkill")
				.WithSkill("email")
				.WithAgent("notInAlarm")
				.WithSkill("phone")
				.WithAgent("expected")
				.WithSkill("phone")
				;
			var wrongSite = Database.PersonIdFor("wrongSite");
			var wrongSkill = Database.PersonIdFor("wrongSkill");
			var notInAlarm = Database.PersonIdFor("notInAlarm");
			var expected = Database.PersonIdFor("expected");
			var site = Database.CurrentSiteId();
			var currentSkillId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { expected, wrongSite, wrongSkill, notInAlarm });
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = expected,
					SiteId = site,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = Guid.NewGuid()
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = wrongSite,
					SiteId = Guid.NewGuid(),
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = Guid.NewGuid()
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = wrongSkill,
					SiteId = site,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = Guid.NewGuid()
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = notInAlarm,
					SiteId = site,
					StateGroupId = Guid.NewGuid()
				});
			});

			WithUnitOfWork.Get(() => Target.ReadInAlarmExcludingPhoneStatesForSitesAndSkill(new[] { site }, new[] { currentSkillId }, new Guid?[] {null}))
				.Single().PersonId.Should().Be(expected);
		}

		[Test]
		public void ShouldNotLoadForPreviousSkill()
		{
			Now.Is("2016-11-08 08:10");
			Database
				.WithSite()
				.WithPerson("agent1")
				.WithPersonPeriod("2016-11-01".Date())
				.WithSkill("email")
				.WithPersonPeriod("2016-11-07".Date())
				.WithSkill("phone")
				.WithPersonPeriod("2016-11-10".Date())
				.WithSkill("email")
				;
			var site = Database.CurrentSiteId();
			var personId = Database.CurrentPersonId();
			var email = Database.SkillIdFor("email");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { personId });
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = personId,
					SiteId = site,
					AlarmStartTime = "2016-11-08 08:00".Utc(),
					StateGroupId = Guid.NewGuid()
				});
			});

			WithUnitOfWork.Get(() => Target.ReadInAlarmExcludingPhoneStatesForSitesAndSkill(new[] {site}, new[] { email }, new Guid?[] {null}))
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldLoadForSitesAndSkillWhenExcludingNull()
		{
			Now.Is("2016-11-07 08:10");
			Database
				.WithSite()
				.WithAgent("expected")
				.WithSkill("phone")
				.WithAgent("wrongSite")
				.WithSkill("phone")
				.WithAgent("wrongState")
				.WithSkill("phone")
				.WithAgent("nullState")
				.WithSkill("phone")
				;

			var siteId = Database.CurrentSiteId();
			var expected = Database.PersonIdFor("expected");
			var wrongSite = Database.PersonIdFor("wrongSite");
			var wrongState = Database.PersonIdFor("wrongState");
			var nullState = Database.PersonIdFor("nullState");
			var skillId = Database.SkillIdFor("phone");
			var loggedOut = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { expected, wrongState, nullState, wrongSite });
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = expected,
					SiteId = siteId,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = Guid.NewGuid()
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = wrongState,
					SiteId = siteId,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = loggedOut
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = nullState,
					SiteId = siteId,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = false,
					StateGroupId = null
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = wrongSite,
					SiteId = Guid.NewGuid(),
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = false,
					StateGroupId = Guid.NewGuid()
				});
			});

			WithUnitOfWork.Get(() => Target.ReadInAlarmExcludingPhoneStatesForSitesAndSkill(new[] { siteId }, new[] { skillId }, new Guid?[] { loggedOut, null }))
				.Single().PersonId.Should().Be(expected);
		}

		[Test]
		public void ShouldLoadForSitesAndSkillWhenIncludingNull()
		{
			Now.Is("2016-11-07 08:10");
			Database
				.WithSite()
				.WithAgent("expected")
				.WithSkill("phone")
				.WithAgent("wrongState")
				.WithSkill("phone")
				;

			var siteId = Database.CurrentSiteId();
			var wrongState = Database.PersonIdFor("wrongState");
			var expected = Database.PersonIdFor("expected");
			var skillId = Database.SkillIdFor("phone");
			var loggedOut = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { wrongState, expected });
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = wrongState,
					SiteId = siteId,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					StateGroupId = loggedOut
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = expected,
					SiteId = siteId,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					StateGroupId = null
				});
			});

			WithUnitOfWork.Get(() => Target.ReadInAlarmExcludingPhoneStatesForSitesAndSkill(new[] { siteId }, new[] { skillId }, new Guid?[] { loggedOut }))
				.Single().PersonId.Should().Be(expected);
		}
	}
}