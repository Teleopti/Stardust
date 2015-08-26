using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core.Providers
{
	public class PeopleSkillUpdater : IPeopleSkillUpdater
	{
		private readonly IPersonRepository _personRepository;
		private readonly IContractRepository _contractRepository;
		private readonly IPartTimePercentageRepository _partTimePercentageRepo;
		private readonly IContractScheduleRepository _contractScheduleRepo;
		private readonly ITeamRepository _teamRepository;
		private readonly ISkillRepository _skillRepository;

		public PeopleSkillUpdater(IPersonRepository personRepository, IContractRepository contractRepository,
			IPartTimePercentageRepository partTimePercentageRepo, IContractScheduleRepository contractScheduleRepo,
			ITeamRepository teamRepository, ISkillRepository skillRepository)
		{
			_personRepository = personRepository;
			_contractRepository = contractRepository;
			_partTimePercentageRepo = partTimePercentageRepo;
			_contractScheduleRepo = contractScheduleRepo;
			_teamRepository = teamRepository;
			_skillRepository = skillRepository;
		}

		public int UpdateSkills(PeopleCommandInput model)
		{
			var personIdList = model.People.Select(p => p.PersonId);
			var persons = _personRepository.FindPeople(personIdList);
			var updatedCount = 0;
			foreach (var person in persons)
			{
				var periods = person.PersonPeriods(new DateOnlyPeriod(new DateOnly(model.Date), new DateOnly(model.Date)));
				var inputSkills = model.People.Single(x => x.PersonId == person.Id).SkillIdList ?? new List<Guid>();
				if (!periods.Any())
				{
					var newPeriod = new PersonPeriod(new DateOnly(model.Date),
						new PersonContract(_contractRepository.LoadAll().FirstOrDefault(),
							_partTimePercentageRepo.LoadAll().FirstOrDefault(), _contractScheduleRepo.LoadAll().FirstOrDefault()),
						_teamRepository.LoadAll().FirstOrDefault());
					addSkillToPeriod(inputSkills, newPeriod);
					person.AddPersonPeriod(newPeriod);
					updatedCount++;
					continue;
				}
				var currentPeriod = periods.First();
				var currentSkills = currentPeriod.PersonSkillCollection.Select(s => s.Skill.Id.GetValueOrDefault()).ToList();
				if (!currentSkills.Except(inputSkills).Any() && !inputSkills.Except(currentSkills).Any())
				{
					continue;
				}
				if (!currentPeriod.StartDate.Equals(new DateOnly(model.Date)))
				{

					var newPeriod = currentPeriod.NoneEntityClone();
					newPeriod.StartDate = new DateOnly(model.Date);
					((IPersonPeriodModifySkills)newPeriod).ResetPersonSkill();
					addSkillToPeriod(inputSkills, newPeriod);
					person.AddPersonPeriod(newPeriod);
					updatedCount++;
					continue;
				}
				((IPersonPeriodModifySkills)currentPeriod).ResetPersonSkill();
				addSkillToPeriod(inputSkills, currentPeriod);
				updatedCount++;
			}

			return updatedCount;
		}

		private void addSkillToPeriod(IEnumerable<Guid> inputSkills, IPersonPeriod newPeriod)
		{
			foreach (var skillId in inputSkills)
			{
				var skill = _skillRepository.Get(skillId);
				((IPersonPeriodModifySkills)newPeriod).AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			}
		}
	}
}