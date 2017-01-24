﻿using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[DatabaseTest]
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
			WithUnitOfWork.Get(() => Target.FetchNumberOfAgents(new Guid[0]))
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

		[Test]
		public void ShouldOnlyCountAgentOnce()
		{
			Database
				.WithAgent()
				.WithSkill("phone")
				.WithSkill("email")
				.UpdateGroupings();
			var teamId = Database.CurrentTeamId();
			var phoneId = Database.SkillIdFor("phone");
			var emailId = Database.SkillIdFor("email");

			var result = WithUnitOfWork.Get(() => Target.ForSkills(new[] { teamId }, new[] { phoneId, emailId }));

			result[teamId].Should().Be(1);
		}
		
		[Test]
		public void ShouldNotCountFutureOrPastPersonPeriodForSkill()
		{
			Now.Is("2016-11-16");
			Database
				.WithPerson("ashley")
				.WithPersonPeriod("2016-09-01")
				.WithSkill("email")
				.WithPersonPeriod("2016-11-01")
				.WithSkill("phone")
				.WithPersonPeriod("2017-01-01")
				.WithSkill("email")
				.UpdateGroupings()
				;
			var teamId = Database.CurrentTeamId();
			var emailId = Database.SkillIdFor("email");

			var result = WithUnitOfWork.Get(() => Target.ForSkills(new[] { teamId }, new[] { emailId }));

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotCountAllPersonPeriodsThatShareSkill()
		{
			Now.Is("2016-11-16");
			Database
				.WithPerson("ashley")
				.WithPersonPeriod("2016-09-01")
				.WithSkill("email")
				.WithPersonPeriod("2016-11-01")
				.WithSkill("email")
				.WithPersonPeriod("2017-01-01")
				.WithSkill("email")
				.UpdateGroupings()
				;
			var teamId = Database.CurrentTeamId();
			var emailId = Database.SkillIdFor("email");

			var result = WithUnitOfWork.Get(() => Target.ForSkills(new[] { teamId }, new[] { emailId }));

			result[teamId].Should().Be(1);
		}


		[Test]
		public void ShouldNotLoadTerminatedAgentForSkill()
		{
			Now.Is("2016-11-16 08:00");
			Database
				.WithAgent()
				.WithSkill("phone")
				.WithTerminatedAgent("2016-11-15")
				.WithSkill("phone")
				.UpdateGroupings();
			var teamId = Database.CurrentTeamId();
			var phoneId = Database.SkillIdFor("phone");

			var result = WithUnitOfWork.Get(() => Target.ForSkills(new[] { teamId }, new[] { phoneId }));

			result[teamId].Should().Be(1);
		}
	}
}