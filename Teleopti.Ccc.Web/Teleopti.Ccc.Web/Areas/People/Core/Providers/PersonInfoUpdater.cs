using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core.Providers
{
	public class PersonInfoUpdater : IPersonInfoUpdater
	{
		private readonly IPersonRepository _personRepository;
		private readonly IContractRepository _contractRepository;
		private readonly IPartTimePercentageRepository _partTimePercentageRepo;
		private readonly IContractScheduleRepository _contractScheduleRepo;
		private readonly ITeamRepository _teamRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly IRuleSetBagRepository _shiftBagRepository;

		public PersonInfoUpdater(IPersonRepository personRepository, IContractRepository contractRepository,
			IPartTimePercentageRepository partTimePercentageRepo, IContractScheduleRepository contractScheduleRepo,
			ITeamRepository teamRepository, ISkillRepository skillRepository, IRuleSetBagRepository shiftBagRepository)
		{
			_personRepository = personRepository;
			_contractRepository = contractRepository;
			_partTimePercentageRepo = partTimePercentageRepo;
			_contractScheduleRepo = contractScheduleRepo;
			_teamRepository = teamRepository;
			_skillRepository = skillRepository;
			_shiftBagRepository = shiftBagRepository;
		}

		public int UpdatePersonInfo(PeopleCommandInput model)
		{
			var personIdList = model.People.Select(p => p.PersonId);
			var persons = _personRepository.FindPeople(personIdList);
			var updatedCount = 0;
			foreach (var person in persons)
			{
				var inputPerson = model.People.Single(x => x.PersonId == person.Id);
				var inputSkills = inputPerson.SkillIdList ?? new List<Guid>();

				var periods = person.PersonPeriods(new DateOnlyPeriod(new DateOnly(model.Date), new DateOnly(model.Date)));
				if (!periods.Any())
				{
					var newPeriod = new PersonPeriod(new DateOnly(model.Date),
						new PersonContract(_contractRepository.LoadAll().FirstOrDefault(),
							_partTimePercentageRepo.LoadAll().FirstOrDefault(), _contractScheduleRepo.LoadAll().FirstOrDefault()),
						_teamRepository.LoadAll().FirstOrDefault());

					updatePeriod(person, inputSkills, newPeriod, inputPerson.ShiftBagId);

					person.AddPersonPeriod(newPeriod);
					updatedCount++;
					continue;
				}
				var currentPeriod = periods.First();
				var currentSkills = currentPeriod.PersonSkillCollection.Select(s => s.Skill.Id.GetValueOrDefault()).ToList();
				if (!currentSkills.Except(inputSkills).Any() && !inputSkills.Except(currentSkills).Any() &&
					!inputPerson.ShiftBagId.HasValue)
				{
					continue;
				}
				if (!currentPeriod.StartDate.Equals(new DateOnly(model.Date)))
				{

					var newPeriod = currentPeriod.NoneEntityClone();
					newPeriod.StartDate = new DateOnly(model.Date);
					person.ResetPersonSkills(newPeriod);

					updatePeriod(person, inputSkills, newPeriod, inputPerson.ShiftBagId);

					person.AddPersonPeriod(newPeriod);
					updatedCount++;
					continue;
				}
				person.ResetPersonSkills(currentPeriod);

				updatePeriod(person, inputSkills, currentPeriod, inputPerson.ShiftBagId);
				updatedCount++;
			}

			return updatedCount;
		}

		private void updatePeriod(IPerson person, IEnumerable<Guid> inputSkills, IPersonPeriod period, Guid? shiftBagId)
		{
			foreach (var skillId in inputSkills)
			{
				var skill = _skillRepository.Get(skillId);
				person.AddSkill(new PersonSkill(skill, new Percent(1)), period);
			}

			if (shiftBagId.HasValue)
			{
				period.RuleSetBag = _shiftBagRepository.Get(shiftBagId.Value);
			}
		}
	}
}