using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Ccc.Web.Areas.People.Core.Models;
using Teleopti.Ccc.WebTest.Areas.People.IoC;


namespace Teleopti.Ccc.WebTest.Areas.People
{
	[WebPeopleTest]
	public class PeopleDataControllerTest
	{
		public PeopleDataController Target;
		public IPersonRepository PersonRepository;
		public ISkillRepository SkillRepository;
		public IRuleSetBagRepository RuleSetBagRepository;
		public IWorkShiftRuleSetRepository WorkShiftRuleSetRepository;

		private IPerson prepareData(DateTime date)
		{
			var person = new Person().WithName(new Name("John", "Smith"));
			person.SetId(Guid.NewGuid());
			PersonRepository.Add(person);

			var period = createPersonPeriod(date);
			person.AddPersonPeriod(period);
			return person;
		}

		private IPersonPeriod createPersonPeriod(DateTime date)
		{
			var team = TeamFactory.CreateSimpleTeam("team");
			SiteFactory.CreateSiteWithTeams(new List<Team> {team});

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

			var result = Target.FetchPeople(new InputModel {Date = date, PersonIdList = new[] {person.Id.Value}});

			result.Content.Count().Should().Be.EqualTo(1);
			var personFirst = result.Content.First();
			personFirst.PersonId.Should().Be.EqualTo(person.Id.Value);
			personFirst.SkillIdList.Count.Should().Be.EqualTo(2);
			personFirst.SkillIdList.Should().Contain(skill.Id.Value);
			personFirst.SkillIdList.Should().Contain(anotherSkill.Id.Value);
			personFirst.ShiftBagId.Should().Be.EqualTo(period.RuleSetBag.Id);
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

		[Test]
		public void ShouldLoadAllShiftBags()
		{
			var workRuleSet = WorkShiftRuleSetFactory.Create();
			WorkShiftRuleSetRepository.Add(workRuleSet);
			var shiftbag1 = new RuleSetBag();
			shiftbag1.SetId(Guid.NewGuid());
			shiftbag1.AddRuleSet(workRuleSet);
			var shiftbag2 = new RuleSetBag();
			shiftbag2.SetId(Guid.NewGuid());
			shiftbag2.AddRuleSet(workRuleSet);

			RuleSetBagRepository.Add(shiftbag1);
			RuleSetBagRepository.Add(shiftbag2);

			var result = Target.LoadAllShiftBags();

			result.Content.Count().Should().Be.EqualTo(2);
			result.Content.First().ShiftBagName.Should().Be.EqualTo(shiftbag1.Description.Name);
			result.Content.Second().ShiftBagName.Should().Be.EqualTo(shiftbag2.Description.Name);
			result.Content.First().ShiftBagId.Should().Be.EqualTo(shiftbag1.Id.Value);
			result.Content.Second().ShiftBagId.Should().Be.EqualTo(shiftbag2.Id.Value);
		}
	}
}
