using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.AgentStateReadModelReader
{
	[DatabaseTest]
	[TestFixture]
	public class WithNames
	{
		public IGroupingReadOnlyRepository Groupings;
		public Database Database;
		public IAgentStateReadModelPersister StatePersister;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IAgentStateReadModelReader Target;

		[Test]
		public void ShouldReadNameForEverything()
		{
			Now.Is("2017-01-23 08:10");
			Database
				.WithSite("site")
				.WithTeam("team")
				.WithAgent("agent one", "1")
				.WithSkill("phone")
				.UpdateGroupings();
			var personId1 = Database.PersonIdFor("agent one");
			var siteId = Database.CurrentSiteId();
			var teamId = Database.CurrentTeamId();
			var skillId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(u =>
			{
				u.Current().FetchSession()
					.CreateSQLQuery($@"
INSERT INTO [ReadModel].[AgentState] 
(
	PersonId,
	SiteId,
	TeamId,
	SiteName,
	TeamName,
	FirstName,
	LastName,
	EmploymentNumber,
	IsRuleAlarm,
	AlarmStartTime,
	StateGroupId
) VALUES (
	'{personId1}',
	'{siteId}',
	'{teamId}',
	'site',
	'team',
	'agent',
	'one',
	'1',
	1,
	'2017-01-23 08:00',
	'{Guid.NewGuid()}'
)
").ExecuteUpdate();
			});

			WithUnitOfWork.Do(() =>
			{
				new[]
				{
					new guidHolder {SiteIds = new[] {siteId}},
					new guidHolder {TeamIds = new[] {teamId}},
					new guidHolder {SiteIds = new[] {siteId}, TeamIds = new[] {teamId}},
					new guidHolder {SkillIds = new[] {skillId}},
					new guidHolder {SiteIds = new[] {siteId}, SkillIds = new[] {skillId}},
					new guidHolder {TeamIds = new[] {teamId}, SkillIds = new[] {skillId}},
					new guidHolder {SiteIds = new[] {siteId}, TeamIds = new[] {teamId}, SkillIds = new[] {skillId}},
				}.ForEach(p =>
				{
					assert(Target.ReadFor(p.SiteIds, p.TeamIds, p.SkillIds));
					assert(Target.ReadInAlarmFor(p.SiteIds, p.TeamIds, p.SkillIds));
					assert(Target.ReadInAlarmExcludingStatesFor(p.SiteIds, p.TeamIds, p.SkillIds, new Guid?[] {Guid.NewGuid()}));
				});
			});
		}

		private static void assert(IEnumerable<AgentStateReadModel> result)
		{
			var model = result.Single();
			model.FirstName.Should().Be("agent");
			model.LastName.Should().Be("one");
			model.EmploymentNumber.Should().Be("1");
			model.SiteName.Should().Be("site");
			model.TeamName.Should().Be("team");
		}

		private class guidHolder
		{
			public IEnumerable<Guid> SiteIds { get; set; }
			public IEnumerable<Guid> TeamIds { get; set; }
			public IEnumerable<Guid> SkillIds { get; set; }
		}
	}
}