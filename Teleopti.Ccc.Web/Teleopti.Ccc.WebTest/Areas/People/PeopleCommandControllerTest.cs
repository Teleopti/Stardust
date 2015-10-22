﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Repositories;
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
		public IRuleSetBagRepository RuleSetBagRepository;
		public IWorkShiftRuleSetRepository WorkShiftRuleSetRepository;

		private IPerson prepareData(DateTime date, IEnumerable<IPersonSkill> personSkills = null)
		{
			var person = new Person { Name = new Name("John", "Smith") };
			person.SetId(Guid.NewGuid());
			PersonRepository.Add(person);

			var period = createPersonPeriod(date, personSkills);
			person.AddPersonPeriod(period);
			return person;
		}

		private IPersonPeriod createPersonPeriod(DateTime date, IEnumerable<IPersonSkill> personSkills)
		{
			var contract = new Contract("test");
			ContractRepository.Add(contract);

			var partTimePercentage = new PartTimePercentage("test");
			PartTimePercentageRepository.Add(partTimePercentage);

			var contractSchdule = new ContractSchedule("test");
			ContractScheduleRepository.Add(contractSchdule);

			var team = new Team();
			TeamRepository.Add(team);

			var newPeriod = new PersonPeriod(new DateOnly(date),
				new PersonContract(contract, partTimePercentage, contractSchdule), team);

			if (personSkills != null)
			{
				foreach (var personSkill in personSkills)
				{
					newPeriod.AddPersonSkill(personSkill);	
				}
			}

			return newPeriod;
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
			Target.UpdatePersonSkills(new PeopleSkillCommandInput
			{
				People = new List<SkillUpdateCommandInputModel> { new SkillUpdateCommandInputModel { PersonId = person.Id.Value } },
				Date = date
			});
			var updatedPerson = PersonRepository.Get(person.Id.GetValueOrDefault());
			updatedPerson.PersonPeriodCollection.Count.Should().Be.EqualTo(2);
			updatedPerson.PersonPeriodCollection.First().StartDate.Date.Should().Be.EqualTo(date);
		}

		[Test]
		public void ShouldCreatePersonPeriodBasedOnCurrentPeriod()
		{
			const double phoneProficiency = 0.42;
			const double emailProficiency = 0.84;

			var date = new DateTime(2015, 8, 20);
			var testDate = date.AddDays(-10);

			var skillPhone = SkillFactory.CreateSkillWithId("phone");
			SkillRepository.Add(skillPhone);

			var skillEmail = SkillFactory.CreateSkillWithId("Email");
			SkillRepository.Add(skillEmail);

			var personSkills = new[]
			{
				new PersonSkill(skillPhone, new Percent(phoneProficiency)),
				new PersonSkill(skillEmail, new Percent(emailProficiency))
			};
			var person = prepareData(testDate, personSkills);
			
			Target.UpdatePersonSkills(new PeopleSkillCommandInput
			{
				People =
					new List<SkillUpdateCommandInputModel>
					{
						new SkillUpdateCommandInputModel
						{
							PersonId = person.Id.Value,
							SkillIdList = new[]
							{
								skillPhone.Id.GetValueOrDefault()
							}
						}
					},
				Date = date
			});
			var updatedPerson = PersonRepository.Get(person.Id.GetValueOrDefault());
			updatedPerson.PersonPeriodCollection.Count.Should().Be.EqualTo(2);
			
			var existingPeriod = updatedPerson.PersonPeriodCollection.First();
			existingPeriod.StartDate.Date.Should().Be.EqualTo(date.AddDays(-10));
			existingPeriod.PersonSkillCollection.Count().Should().Be.EqualTo(2);

			var newCreatedPeriod = updatedPerson.PersonPeriodCollection.Second();
			newCreatedPeriod.StartDate.Date.Should().Be.EqualTo(date);
			newCreatedPeriod.PersonSkillCollection.Count().Should().Be(1);

			var personSkillInNewPeriod = newCreatedPeriod.PersonSkillCollection.Single();
			personSkillInNewPeriod.Skill.Id.Should().Be.EqualTo(skillPhone.Id);
			personSkillInNewPeriod.Active.Should().Be.EqualTo(true);
			personSkillInNewPeriod.SkillPercentage.Should().Be.EqualTo(new Percent(phoneProficiency));
		}

		[Test]
		public void ShouldNotCreatePersonPeriodWhenCurrentOneStartFromGivenDate()
		{
			var date = new DateTime(2015, 8, 20);
			var person = prepareData(date);
			Target.UpdatePersonSkills(new PeopleSkillCommandInput
			{
				People = new List<SkillUpdateCommandInputModel> { new SkillUpdateCommandInputModel { PersonId = person.Id.Value } },
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
			Target.UpdatePersonSkills(new PeopleSkillCommandInput
			{
				People = new List<SkillUpdateCommandInputModel> { new SkillUpdateCommandInputModel { PersonId = person.Id.Value } },
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
			Target.UpdatePersonSkills(new PeopleSkillCommandInput
			{
				People =
					new List<SkillUpdateCommandInputModel>
					{
						new SkillUpdateCommandInputModel {PersonId = person.Id.Value, SkillIdList = new List<Guid> {skill.Id.Value}}
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
			var result = Target.UpdatePersonSkills(new PeopleSkillCommandInput
			{
				People =
					new List<SkillUpdateCommandInputModel>
					{
						new SkillUpdateCommandInputModel {PersonId = person.Id.Value, SkillIdList = new List<Guid> {skill.Id.Value}}
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

		[Test]
		public void ShouldAddShiftBagToSamePeriodIfSameStartDate()
		{
			var date = new DateTime(2015, 8, 21);
			var person = prepareData(date);
			var workRuleSet = WorkShiftRuleSetFactory.Create();
			WorkShiftRuleSetRepository.Add(workRuleSet);

			var shiftbag = new RuleSetBag();
			shiftbag.SetId(Guid.NewGuid());
			shiftbag.AddRuleSet(workRuleSet);
			RuleSetBagRepository.Add(shiftbag);

			var result = Target.UpdatePersonShiftBag(new PeopleShiftBagCommandInput
			{
				People =
					new List<ShiftBagUpdateCommandInputModel>
					{
						new ShiftBagUpdateCommandInputModel
						{
							PersonId = person.Id.Value,
							ShiftBagId = shiftbag.Id.Value
						}
					},
				Date = date
			});
			var updatedPerson = PersonRepository.Get(person.Id.GetValueOrDefault());
			updatedPerson.PersonPeriodCollection.Count.Should().Be.EqualTo(1);
			var personPeriod = updatedPerson.PersonPeriods(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date))).First();
			personPeriod.RuleSetBag.Id.Should().Be.EqualTo(shiftbag.Id.Value);
			result.Content.Success.Should().Be.EqualTo(true);
			result.Content.SuccessCount.Should().Be.EqualTo(1);
		}
	}
}
