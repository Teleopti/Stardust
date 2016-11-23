using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta.AgentStateReadModelReader.Consolidated
{
	[DatabaseTest]
	[TestFixture]
	[Toggle(Toggles.RTA_QuicklyChangeAgentsSelection_40610)]
	public class ForCombinations
	{
		public IGroupingReadOnlyRepository Groupings;
		public Database Database;
		public IAgentStateReadModelPersister StatePersister;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IAgentStateReadModelReader Target;

		[Test]
		public void ShouldLoadForSiteAndTeam()
		{
			Now.Is("2016-11-07 08:00");
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = personId1,
					SiteId = siteId
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = personId2,
					TeamId = teamId
				});
			});

			WithUnitOfWork.Get(() => Target.ReadFor(new[] {siteId}, new[] {teamId}, null))
				.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(new[] {personId1, personId2});
		}

		[Test]
		public void ShouldLoadForSiteTeamAndSkill()
		{
			Now.Is("2016-11-07 08:00");
			Database
				.WithAgent("wrongTeam")
				.WithSkill("phone")
				.WithTeam()
				.WithAgent("wrongSite")
				.WithSkill("phone")
				.WithSite()
				.WithAgent("expectedForSite")
				.WithSkill("phone")
				.WithAgent("expectedForTeam")
				.WithSkill("phone")
				.WithAgent("wrongSkill")
				.WithSkill("email")
				.UpdateGroupings()
				;
			var expectedForSite = Database.PersonIdFor("expectedForSite");
			var expectedForTeam = Database.PersonIdFor("expectedForTeam");
			var wrongSkill = Database.PersonIdFor("wrongSkill");
			var wrongSite = Database.PersonIdFor("wrongSite");
			var wrongTeam = Database.PersonIdFor("wrongTeam");
			var siteId = Database.CurrentSiteId();
			var teamId = Database.CurrentSiteId();
			var phoneId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = expectedForSite,
					SiteId = siteId
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = expectedForTeam,
					TeamId = teamId
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = wrongSkill,
					SiteId = siteId,
					TeamId = teamId
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = wrongSite,
					SiteId = Guid.NewGuid()
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = wrongTeam,
					TeamId = Guid.NewGuid()
				});
			});

			WithUnitOfWork.Get(() => Target.ReadFor(new[] {siteId}, new []{teamId}, new[] {phoneId}))
				.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(expectedForSite, expectedForTeam);
		}
	}
}