using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.People
{
	[PeopleCommandTest]
	public class PeopleCommandControllerTest
	{
		public PeopleCommandController Target;
		public IPersonRepository PersonRepository;
		public IContractRepository ContractRepository;
		public IPartTimePercentageRepository PartTimePercentageRepository;
		public IContractScheduleRepository ContractScheduleRepository;
		public ITeamRepository TeamRepository;
		public ISkillRepository SkillRepository;

		private IPerson prepareData(DateTime date)
		{
			var person = new Person {Name = new Name("John", "Smith")};
			person.SetId(Guid.NewGuid());
			PersonRepository.Add(person);

			var period = createPersonPeriod(date);
			person.AddPersonPeriod(period);
			return person;
		}

		private IPersonPeriod createPersonPeriod(DateTime date)
		{
			var contract = new Contract("test");
			ContractRepository.Add(contract);

			var partTimePercentage = new PartTimePercentage("test");
			PartTimePercentageRepository.Add(partTimePercentage);

			var contractSchdule = new ContractSchedule("test");
			ContractScheduleRepository.Add(contractSchdule);

			var team = new Team();
			TeamRepository.Add(team);

			return new PersonPeriod(new DateOnly(date), new PersonContract(contract, partTimePercentage, contractSchdule), team);
		}

		[Test]
		public void TargetShouldNotBeNull()
		{
			Target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldCreateNewPersonPeriodOnPersonWithoutPersonPeriod()
		{
			var date = new DateTime(2015, 8, 20);
			var person = prepareData(date.AddDays(10));
			Target.UpdateSkill(new PeopleCommandInput
			{
				People = new List<PersonSkillModel> {new PersonSkillModel {PersonId = person.Id.Value}},
				Date = date
			});
			var updatedPerson = PersonRepository.Get(person.Id.GetValueOrDefault());
			updatedPerson.PersonPeriodCollection.Count.Should().Be.EqualTo(2);
			updatedPerson.PersonPeriodCollection.First().StartDate.Date.Should().Be.EqualTo(date);
		}

		[Test]
		public void ShouldCreatePersonPeriodBasedOnCurrentPeriod()
		{
			var date = new DateTime(2015, 8, 20);
			var person = prepareData(date.AddDays(-10));
			var skill = SkillFactory.CreateSkillWithId("phone");
			SkillRepository.Add(skill);
			Target.UpdateSkill(new PeopleCommandInput
			{
				People =
					new List<PersonSkillModel>
					{
						new PersonSkillModel {PersonId = person.Id.Value, SkillIdList = new[] {skill.Id.GetValueOrDefault()}}
					},
				Date = date
			});
			var updatedPerson = PersonRepository.Get(person.Id.GetValueOrDefault());
			updatedPerson.PersonPeriodCollection.Count.Should().Be.EqualTo(2);
			updatedPerson.PersonPeriodCollection.First().StartDate.Date.Should().Be.EqualTo(date.AddDays(-10));
			updatedPerson.PersonPeriodCollection.Second().StartDate.Date.Should().Be.EqualTo(date);
		}

		[Test]
		public void ShouldNotCreatePersonPeriodWhenCurrentOneStartFromGivenDate()
		{
			var date = new DateTime(2015, 8, 20);
			var person = prepareData(date);
			Target.UpdateSkill(new PeopleCommandInput
			{
				People = new List<PersonSkillModel> {new PersonSkillModel {PersonId = person.Id.Value}},
				Date = date
			});
			var updatedPerson = PersonRepository.Get(person.Id.GetValueOrDefault());
			updatedPerson.PersonPeriodCollection.Count.Should().Be.EqualTo(1);
			updatedPerson.PersonPeriodCollection.First().StartDate.Date.Should().Be.EqualTo(date);
		}

		[Test]
		public void ShouldNotCreatePersonPeriodWhenPersonTerminated()
		{
			var date = new DateTime(2015, 8, 20);
			var person = prepareData(date.AddDays(-2));
			person.TerminatePerson(new DateOnly(date.AddDays(-1)), new PersonAccountUpdaterDummy());
			Target.UpdateSkill(new PeopleCommandInput
			{
				People = new List<PersonSkillModel> {new PersonSkillModel {PersonId = person.Id.Value}},
				Date = date
			});
			var updatedPerson = PersonRepository.Get(person.Id.GetValueOrDefault());
			updatedPerson.PersonPeriodCollection.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldAddSkillToPersonNotHavingBefore()
		{
			var date = new DateTime(2015, 8, 20);
			var person = prepareData(date.AddDays(-10));
			var skill = SkillFactory.CreateSkillWithId("phone");
			SkillRepository.Add(skill);
			Target.UpdateSkill(new PeopleCommandInput
			{
				People =
					new List<PersonSkillModel>
					{
						new PersonSkillModel {PersonId = person.Id.Value, SkillIdList = new List<Guid> {skill.Id.Value}}
					},
				Date = date
			});
			var updatedPerson = PersonRepository.Get(person.Id.GetValueOrDefault());
			updatedPerson.PersonPeriodCollection.Count.Should().Be.EqualTo(2);
			var personPeriod = updatedPerson.PersonPeriods(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date))).First();
			personPeriod.PersonSkillCollection.First().Skill.Id.Should().Be.EqualTo(skill.Id);
		}

		[Test]
		public void ShouldAddSkillToSamePeriodIfSameStartDate()
		{
			var date = new DateTime(2015, 8, 20);
			var person = prepareData(date);
			var skill = SkillFactory.CreateSkillWithId("phone");
			SkillRepository.Add(skill);
			var result = Target.UpdateSkill(new PeopleCommandInput
			{
				People =
					new List<PersonSkillModel>
					{
						new PersonSkillModel {PersonId = person.Id.Value, SkillIdList = new List<Guid> {skill.Id.Value}}
					},
				Date = date
			});
			var updatedPerson = PersonRepository.Get(person.Id.GetValueOrDefault());
			updatedPerson.PersonPeriodCollection.Count.Should().Be.EqualTo(1);
			var personPeriod = updatedPerson.PersonPeriods(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date))).First();
			personPeriod.PersonSkillCollection.First().Skill.Id.Should().Be.EqualTo(skill.Id);
			result.Content.Success.Should().Be.EqualTo(true);
			result.Content.SuccessCount.Should().Be.EqualTo(1);
		}
	}
}
