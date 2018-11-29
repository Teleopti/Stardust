using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.People.Core.Models;


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

		public int UpdatePersonSkill(PeopleSkillCommandInput model)
		{
			var personIdList = model.People.Select(p => p.PersonId);
			var persons = _personRepository.FindPeople(personIdList);
			var peopleLookup = model.People.ToLookup(p => p.PersonId);
			var startDate = new DateOnly(model.Date);
			var updatedCount = 0;
			foreach (var person in persons)
			{
				var inputPerson = peopleLookup[person.Id.GetValueOrDefault()].Single();
				var inputSkills = inputPerson.SkillIdList ?? new List<Guid>();

				var currentPeriod = person.Period(startDate);
				if (currentPeriod == null)
				{
					var newPeriod = new PersonPeriod(startDate,
						new PersonContract(_contractRepository.LoadAll().FirstOrDefault(),
							_partTimePercentageRepo.LoadAll().FirstOrDefault(), _contractScheduleRepo.LoadAll().FirstOrDefault()),
						_teamRepository.LoadAll().FirstOrDefault());

					updatePeriodWithSkill(person, inputSkills, newPeriod);

					person.AddPersonPeriod(newPeriod);
					updatedCount++;
					continue;
				}

				var currentSkills = currentPeriod.PersonSkillCollection.Select(s => s.Skill.Id.GetValueOrDefault()).ToList();
				if (!currentSkills.Except(inputSkills).Any() && !inputSkills.Except(currentSkills).Any())
				{
					continue;
				}

				if (!currentPeriod.StartDate.Equals(startDate))
				{

					var newPeriod = currentPeriod.NoneEntityClone();
					newPeriod.StartDate = startDate;
					person.ResetPersonSkills(newPeriod);

					updatePeriodWithSkill(person, inputSkills, newPeriod, currentPeriod.PersonSkillCollection);

					person.AddPersonPeriod(newPeriod);
					updatedCount++;
					continue;
				}
				var currentSkillCollection = currentPeriod.PersonSkillCollection;
				person.ResetPersonSkills(currentPeriod);

				updatePeriodWithSkill(person, inputSkills, currentPeriod, currentSkillCollection);
				updatedCount++;
			}

			return updatedCount;
		}

		public int UpdatePersonShiftBag(PeopleShiftBagCommandInput model)
		{
			var personIdList = model.People.Select(p => p.PersonId);
			var peopleLookup = model.People.ToLookup(p => p.PersonId);
			var persons = _personRepository.FindPeople(personIdList);
			var startDate = new DateOnly(model.Date);
			var updatedCount = 0;
			foreach (var person in persons)
			{
				var inputPerson = peopleLookup[person.Id.GetValueOrDefault()].Single();

				var inputShiftBag = _shiftBagRepository.Get(inputPerson.ShiftBagId.GetValueOrDefault());
				var currentPeriod = person.Period(startDate);
				if (currentPeriod == null)
				{
					var contract = new PersonContract(_contractRepository.LoadAll().FirstOrDefault(),
						_partTimePercentageRepo.LoadAll().FirstOrDefault(), _contractScheduleRepo.LoadAll().FirstOrDefault());
					var newPeriod = new PersonPeriod(startDate, contract, _teamRepository.LoadAll().FirstOrDefault())
					{
						RuleSetBag = inputShiftBag
					};

					person.AddPersonPeriod(newPeriod);
					updatedCount++;
					continue;
				}

				if ((currentPeriod.RuleSetBag == null && inputShiftBag == null) ||
					(currentPeriod.RuleSetBag != null &&
					 (currentPeriod.RuleSetBag.Id.GetValueOrDefault() == inputShiftBag.Id.GetValueOrDefault())))
				{
					continue;
				}

				if (!currentPeriod.StartDate.Equals(startDate))
				{
					var newPeriod = currentPeriod.NoneEntityClone();
					newPeriod.StartDate = startDate;

					newPeriod.RuleSetBag = inputShiftBag;

					person.AddPersonPeriod(newPeriod);
					updatedCount++;
					continue;
				}

				currentPeriod.RuleSetBag = inputShiftBag;
				updatedCount++;
			}

			return updatedCount;
		}

		private void updatePeriodWithSkill(IPerson person, IEnumerable<Guid> inputSkills, IPersonPeriod period,
			IEnumerable<IPersonSkill> currentExistingSkills = null)
		{
			var currentSkills = currentExistingSkills?.ToList() ?? new List<IPersonSkill>();
			foreach (var skillId in inputSkills)
			{
				var currentSkill = currentSkills.SingleOrDefault(s => s.Skill.Id == skillId);
				var proficiency = currentSkill?.SkillPercentage ?? new Percent(1);

				var skill = _skillRepository.Get(skillId);
				var personSkill = new PersonSkill(skill, proficiency)
				{
					Active = currentSkill == null || currentSkill.Active
				};

				person.AddSkill(personSkill, period);
			}
		}
	}
}