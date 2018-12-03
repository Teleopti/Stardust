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
			var person = new Person().WithName(new Name("John", "Smith"));
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
				People = new List<SkillUpdateCommandInputModel>
				{
					new SkillUpdateCommandInputModel {PersonId = person.Id.GetValueOrDefault()}
				},
				Date = date
			});
			var updatedPerson = PersonRepository.Get(person.Id.GetValueOrDefault());
			updatedPerson.PersonPeriodCollection.Count.Should().Be.EqualTo(2);
			updatedPerson.PersonPeriodCollection.First().StartDate.Date.Should().Be.EqualTo(date);
		}

		[Test]
		public void ShouldDoNothingWhenSkillListNotChanged()
		{
			const double phoneProficiency = 0.42;
			const double emailProficiency = 0.84;

			var date = new DateTime(2015, 8, 20);
			var testDate = date.AddDays(-10);

			var phoneSkill = SkillFactory.CreateSkillWithId("phone");
			var phoneSkillId = phoneSkill.Id.GetValueOrDefault();
			SkillRepository.Add(phoneSkill);

			var emailSkill = SkillFactory.CreateSkillWithId("Email");
			var emailSkillId = emailSkill.Id.GetValueOrDefault();
			SkillRepository.Add(emailSkill);

			var personSkills = new[]
			{
				new PersonSkill(phoneSkill, new Percent(phoneProficiency))
				{
					Active = true
				},
				new PersonSkill(emailSkill, new Percent(emailProficiency))
				{
					Active = false
				}
			};
			var person = prepareData(testDate, personSkills);
			var personId = person.Id.GetValueOrDefault();

			Target.UpdatePersonSkills(new PeopleSkillCommandInput
			{
				People =
					new List<SkillUpdateCommandInputModel>
					{
						new SkillUpdateCommandInputModel
						{
							PersonId = personId,
							SkillIdList = new[] {phoneSkillId, emailSkillId}
						}
					},
				Date = date
			});
			var updatedPerson = PersonRepository.Get(personId);
			updatedPerson.PersonPeriodCollection.Count.Should().Be.EqualTo(1);

			var existingPeriod = updatedPerson.PersonPeriodCollection.Single();
			existingPeriod.StartDate.Date.Should().Be.EqualTo(date.AddDays(-10));
			existingPeriod.PersonSkillCollection.Count().Should().Be.EqualTo(2);

			var firstPersonSkill = existingPeriod.PersonSkillCollection.First();
			firstPersonSkill.Skill.Id.Should().Be.EqualTo(phoneSkill.Id);
			firstPersonSkill.Active.Should().Be.EqualTo(true);
			firstPersonSkill.SkillPercentage.Should().Be.EqualTo(new Percent(phoneProficiency));

			var secondPersonSkill = existingPeriod.PersonSkillCollection.Second();
			secondPersonSkill.Skill.Id.Should().Be.EqualTo(emailSkill.Id);
			secondPersonSkill.Active.Should().Be.EqualTo(false);
			secondPersonSkill.SkillPercentage.Should().Be.EqualTo(new Percent(emailProficiency));
		}

		[Test]
		public void ShouldCreatePersonPeriodBasedOnCurrentPeriod()
		{
			const double phoneProficiency = 0.42;
			const double emailProficiency = 0.84;

			var date = new DateTime(2015, 8, 20);
			var testDate = date.AddDays(-10);

			var phoneSkill = SkillFactory.CreateSkillWithId("phone");
			var phoneSkillId = phoneSkill.Id.GetValueOrDefault();
			SkillRepository.Add(phoneSkill);

			var emailSkill = SkillFactory.CreateSkillWithId("Email");
			SkillRepository.Add(emailSkill);

			var personSkills = new[]
			{
				new PersonSkill(phoneSkill, new Percent(phoneProficiency))
				{
					Active = false
				},
				new PersonSkill(emailSkill, new Percent(emailProficiency))
				{
					Active = true
				}
			};
			var person = prepareData(testDate, personSkills);
			var personId = person.Id.GetValueOrDefault();

			Target.UpdatePersonSkills(new PeopleSkillCommandInput
			{
				People =
					new List<SkillUpdateCommandInputModel>
					{
						new SkillUpdateCommandInputModel
						{
							PersonId = personId,
							SkillIdList = new[] {phoneSkillId}
						}
					},
				Date = date
			});
			var updatedPerson = PersonRepository.Get(personId);
			updatedPerson.PersonPeriodCollection.Count.Should().Be.EqualTo(2);

			var existingPeriod = updatedPerson.PersonPeriodCollection.First();
			existingPeriod.StartDate.Date.Should().Be.EqualTo(date.AddDays(-10));
			existingPeriod.PersonSkillCollection.Count().Should().Be.EqualTo(2);

			var newCreatedPeriod = updatedPerson.PersonPeriodCollection.Second();
			newCreatedPeriod.StartDate.Date.Should().Be.EqualTo(date);
			newCreatedPeriod.PersonSkillCollection.Count().Should().Be(1);

			var personSkillInNewPeriod = newCreatedPeriod.PersonSkillCollection.Single();
			personSkillInNewPeriod.Skill.Id.Should().Be.EqualTo(phoneSkillId);
			personSkillInNewPeriod.Active.Should().Be.EqualTo(false);
			personSkillInNewPeriod.SkillPercentage.Should().Be.EqualTo(new Percent(phoneProficiency));
		}

		[Test]
		public void ShouldNotCreatePersonPeriodWhenCurrentOneStartFromGivenDate()
		{
			const double phoneProficiency = 0.42;
			const double emailProficiency = 0.84;

			var date = new DateTime(2015, 8, 20);
			var phoneSkill = SkillFactory.CreateSkillWithId("phone");
			var phoneSkillId = phoneSkill.Id.GetValueOrDefault();
			SkillRepository.Add(phoneSkill);

			var emailSkill = SkillFactory.CreateSkillWithId("Email");
			SkillRepository.Add(emailSkill);

			var personSkills = new[]
			{
				new PersonSkill(phoneSkill, new Percent(phoneProficiency))
				{
					Active = false
				},
				new PersonSkill(emailSkill, new Percent(emailProficiency))
				{
					Active = true
				}
			};
			var person = prepareData(date, personSkills);
			Target.UpdatePersonSkills(new PeopleSkillCommandInput
			{
				People = new List<SkillUpdateCommandInputModel>
				{
					new SkillUpdateCommandInputModel
					{
						PersonId = person.Id.GetValueOrDefault(),
						SkillIdList = new []{phoneSkillId}
					}
				},
				Date = date
			});
			var updatedPerson = PersonRepository.Get(person.Id.GetValueOrDefault());
			updatedPerson.PersonPeriodCollection.Count.Should().Be.EqualTo(1);
			updatedPerson.PersonPeriodCollection.First().StartDate.Date.Should().Be.EqualTo(date);

			var updatedSkill = updatedPerson.PersonPeriodCollection.First().PersonSkillCollection.Single();
			updatedSkill.SkillPercentage.Should().Be.EqualTo(new Percent(phoneProficiency));
			updatedSkill.Active.Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldNotCreatePersonPeriodWhenPersonTerminated()
		{
			var date = new DateTime(2015, 8, 20);
			var person = prepareData(date.AddDays(-2));
			person.TerminatePerson(new DateOnly(date.AddDays(-1)), new PersonAccountUpdaterDummy());
			Target.UpdatePersonSkills(new PeopleSkillCommandInput
			{
				People = new List<SkillUpdateCommandInputModel>
				{
					new SkillUpdateCommandInputModel
					{
						PersonId = person.Id.GetValueOrDefault()
					}
				},
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
						new SkillUpdateCommandInputModel
						{
							PersonId = person.Id.GetValueOrDefault(),
							SkillIdList = new List<Guid> {skill.Id.GetValueOrDefault()}
						}
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
						new SkillUpdateCommandInputModel
						{
							PersonId = person.Id.GetValueOrDefault(),
							SkillIdList = new List<Guid> {skill.Id.GetValueOrDefault()}
						}
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

			var shiftbag = new RuleSetBag().WithId();
			shiftbag.AddRuleSet(workRuleSet);
			RuleSetBagRepository.Add(shiftbag);

			var result = Target.UpdatePersonShiftBag(new PeopleShiftBagCommandInput
			{
				People =
					new List<ShiftBagUpdateCommandInputModel>
					{
						new ShiftBagUpdateCommandInputModel
						{
							PersonId = person.Id.GetValueOrDefault(),
							ShiftBagId = shiftbag.Id.GetValueOrDefault()
						}
					},
				Date = date
			});
			var updatedPerson = PersonRepository.Get(person.Id.GetValueOrDefault());
			updatedPerson.PersonPeriodCollection.Count.Should().Be.EqualTo(1);
			var personPeriod = updatedPerson.Period(new DateOnly(date));
			personPeriod.RuleSetBag.Id.Should().Be.EqualTo(shiftbag.Id.GetValueOrDefault());
			result.Content.Success.Should().Be.EqualTo(true);
			result.Content.SuccessCount.Should().Be.EqualTo(1);
		}
	}
}
