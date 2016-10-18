using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[InfrastructureTest]
	[TestFixture]
	public class NumberOfAgentsInTeamReaderTest
	{
		public MutableNow Now;
		public Database Database;
		public WithUnitOfWork WithUnitOfWork;
		public INumberOfAgentsInTeamReader Target;

		[Test]
		public void ShouldLoadNumberOfAgentsForTeam()
		{
			Database
				.WithAgent()
				.WithAgent();
			var teamId = Database.CurrentTeamId();

			var result = WithUnitOfWork.Get(() => Target.FetchNumberOfAgents(new[] { teamId }));

			result[teamId].Should().Be(2);
		}

		[Test]
		public void ShouldNotLoadTerminatedAgent()
		{
			Now.Is("2016-10-18 08:00");
			Database
				.WithAgent()
				.WithTerminatedAgent("2016-10-18");
			var teamId = Database.CurrentTeamId();

			var result = WithUnitOfWork.Get(() => Target.FetchNumberOfAgents(new[] { teamId }));

			result[teamId].Should().Be(1);
		}

		[Test]
		public void ShouldReturnEmptyListWhenTeamIsEmpty()
		{
			WithUnitOfWork.Get(() => Target.FetchNumberOfAgents(new Guid[] {}))
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldLoadForSkill()
		{
			Database
				.WithAgent()
				.WithSkill("phone")
				.UpdateGroupings();
			var teamId = Database.CurrentTeamId();
			var skillId = Database.SkillIdFor("phone");

			var result = WithUnitOfWork.Get(() => Target.ForSkills(new[] { teamId }, new[] { skillId }));

			result[teamId].Should().Be(1);
		}
		
		[Test]
		public void ShouldNotReturnSiteWithoutAgents()
		{
			WithUnitOfWork.Get(() => Target.ForSkills(new[] { Guid.NewGuid() }, new[] { Guid.NewGuid() }))
				.Should().Be.Empty();
		}


		[Test]
		public void ShouldOnlyLoadForWantedSkill()
		{
			Database
				.WithAgent()
				.WithSkill("phone")
				.WithAgent()
				.WithSkill("email")
				.UpdateGroupings();
			var teamId = Database.CurrentTeamId();
			var skillId = Database.SkillIdFor("phone");

			var result = WithUnitOfWork.Get(() => Target.ForSkills(new[] { teamId }, new[] { skillId }));

			result[teamId].Should().Be(1);
		}

	}
}