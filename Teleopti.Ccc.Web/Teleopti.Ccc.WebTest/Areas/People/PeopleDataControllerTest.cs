﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.People
{
	[PeopleCommandTest]
	public class PeopleDataControllerTest
	{
		public PeopleDataController Target;
		public IPersonRepository PersonRepository;
		public ISkillRepository SkillRepository;

		private IPerson prepareData(DateTime date)
		{
			var person = new Person();
			person.Name = new Name("John", "Smith");
			person.SetId(Guid.NewGuid());
			PersonRepository.Add(person);

			var period = createPersonPeriod(date);
			person.AddPersonPeriod(period);
			return person;
		}

		private IPersonPeriod createPersonPeriod(DateTime date)
		{
			var team = TeamFactory.CreateSimpleTeam("team");
			var site = SiteFactory.CreateSiteWithTeams(new List<Team> {team});
            var contract = new Contract("contract");
			var partTimePercentage = new PartTimePercentage("partTimePercentage");
			var contractSchdule = new ContractSchedule("contractSchedule");

			var shiftBag = new RuleSetBag {Description = new Description("shiftbag")};

			return new PersonPeriod(new DateOnly(date), new PersonContract(contract, partTimePercentage, contractSchdule), team)
			{
				RuleSetBag = shiftBag
			};
		}

		[Test]
		public void ShouldFetchDataOnDate()
		{
			var date = new DateTime(2015, 8, 21);
			var person = prepareData(date.AddDays(-10));
			var period = createPersonPeriod(date.AddDays(-5));
			person.AddPersonPeriod(period);
			var skill = SkillFactory.CreateSkillWithId("phone");
			person.AddSkill(new PersonSkill(skill, new Percent(1)), period);
			var anotherSkill = SkillFactory.CreateSkillWithId("email");
			person.AddSkill(new PersonSkill(anotherSkill, new Percent(1)), period);
			var result = Target.FetchPeople(new InputModel {Date = date, PersonIdList = new [] {person.Id.Value}});

			result.Content.Count().Should().Be.EqualTo(1);
			var personFirst = result.Content.First();
			personFirst.PersonId.Should().Be.EqualTo(person.Id.Value);
			personFirst.SkillIdList.Count.Should().Be.EqualTo(2);
			personFirst.SkillIdList.Should().Contain(skill.Id.Value);
			personFirst.SkillIdList.Should().Contain(anotherSkill.Id.Value);
			personFirst.ShiftBag.Should().Be.EqualTo(period.RuleSetBag.Description.Name);
		}

		[Test]
		public void ShouldLoadAllSkills()
		{
			var skill1 = SkillFactory.CreateSkillWithId("phone");
			var skill2 = SkillFactory.CreateSkillWithId("email");
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);
			var result = Target.LoadAllSkills();
			result.Content.Count().Should().Be.EqualTo(2);
			result.Content.First().SkillName.Should().Be.EqualTo(skill1.Name);
			result.Content.Second().SkillName.Should().Be.EqualTo(skill2.Name);
			result.Content.First().SkillId.Should().Be.EqualTo(skill1.Id.Value);
			result.Content.Second().SkillId.Should().Be.EqualTo(skill2.Id.Value);
		}
	}
}
