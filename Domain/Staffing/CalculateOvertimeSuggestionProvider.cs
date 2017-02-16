using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class CalculateOvertimeSuggestionProvider
	{
		private readonly IPersonForOvertimeProvider _personProviderForOvertime;
		private readonly IPersonRepository _personRepository;
		private readonly ISkillStaffingIntervalProvider _skillStaffingIntervalProvider;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly ISkillRepository _skillRepository;

		public CalculateOvertimeSuggestionProvider(IPersonForOvertimeProvider personProviderForOvertime,
			IPersonRepository personRepository, ISkillStaffingIntervalProvider skillStaffingIntervalProvider, 
			ISkillCombinationResourceRepository skillCombinationResourceRepository, ISkillRepository skillRepository)
		{
			_personProviderForOvertime = personProviderForOvertime;
			_personRepository = personRepository;
			_skillStaffingIntervalProvider = skillStaffingIntervalProvider;

			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_skillRepository = skillRepository;
		}

		public IList<SkillStaffingInterval> GetOvertimeSuggestions(IList<Guid> skillIds, DateTime startDateTime, DateTime endDateTime )
		{
			// persons that we maybe can add overtime to
			// sorted by the biggest difference between weekly target and actually scheduled, maybe
			var personsModels = _personProviderForOvertime.Persons(skillIds, startDateTime, endDateTime);
			var persons = _personRepository.FindPeople(personsModels.Select(x => x.PersonId));
			//load the persons with the data needed, personskills for example
			var skills = _skillRepository.LoadAllSkills().Where(x => skillIds.Contains(x.Id.GetValueOrDefault()));

			var period = new DateTimePeriod(startDateTime, endDateTime);

			// load skillcombinationdata
			var resources = _skillCombinationResourceRepository.LoadSkillCombinationResources(period).ToList();
			var intervals = _skillStaffingIntervalProvider.GetSkillStaffIntervalsAllSkills(period, resources);

			//börja med skill som har öppet längst på längsta workload
			foreach (var skill in skills)
			{
				//if (!intervals.Any(x => x.SkillId == skill.Id.GetValueOrDefault() && x.AbsoluteDifference < 0)) continue;
				var act = skill.Activity;
				foreach (var person in persons)
				{
					if (!intervals.Any(x => x.SkillId == skill.Id.GetValueOrDefault() && x.AbsoluteDifference < 0)) continue;

					var personSkills = person.Period(new DateOnly(startDateTime)).PersonSkillCollection.Select(x => x.Skill);
					if (!personSkills.Select(x => x.Id).Contains(skill.Id.GetValueOrDefault())) continue;

					var skillsForActivity = personSkills.Where(x => x.Activity == act).Select(y => y.Id.GetValueOrDefault());
					//assume whole resource
					var relevantResources = resources.Where(x => x.SkillCombination.NonSequenceEquals(skillsForActivity) && x.EndDateTime > startDateTime && x.StartDateTime < endDateTime);
			
					relevantResources.ForEach(x => x.Resource += 1);
					intervals = _skillStaffingIntervalProvider.GetSkillStaffIntervalsAllSkills(period, resources);

				}
			}


			//foreach pers

			//loopar
			//Ändra resources - lägg på övertiden på relevanta mha öt-intervall och skills och act
			//validera /kolla om underbemannat


			// filter persons smart when demand is filled on A and there is still demand on B
			// so we just try on add overtime that has Skill B
			
			// loop in some way and add overtime, then check if the demand is fulfilled

			return intervals;
		}
	}
	public class SkillIntervalsForOvertime
	{
		public Guid SkillId { get; set; }
		public DateTime Time { get; set; }
		public double NewStaffing { get; set; }
	}
}