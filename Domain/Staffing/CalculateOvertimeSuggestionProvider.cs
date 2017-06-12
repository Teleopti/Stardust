using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class CalculateOvertimeSuggestionProvider
	{
		private readonly IPersonForOvertimeProvider _personProviderForOvertime;
		private readonly IPersonRepository _personRepository;
		private readonly SkillStaffingIntervalProvider _skillStaffingIntervalProvider;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly ISkillRepository _skillRepository;

		public CalculateOvertimeSuggestionProvider(IPersonForOvertimeProvider personProviderForOvertime,
			IPersonRepository personRepository, SkillStaffingIntervalProvider skillStaffingIntervalProvider, 
			ISkillCombinationResourceRepository skillCombinationResourceRepository, ISkillRepository skillRepository)
		{
			_personProviderForOvertime = personProviderForOvertime;
			_personRepository = personRepository;
			_skillStaffingIntervalProvider = skillStaffingIntervalProvider;

			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_skillRepository = skillRepository;
		}

		public OverTimeStaffingSuggestionModel GetOvertimeSuggestions(IList<Guid> skillIds, DateTime startDateTime, DateTime endDateTime )
		{
			// persons that we maybe can add overtime to
			// sorted by the biggest difference between weekly target and actually scheduled, maybe
			var personsModels = _personProviderForOvertime.Persons(skillIds, startDateTime, endDateTime);
			var persons = _personRepository.FindPeople(personsModels.Select(x => x.PersonId));
			var skills = _skillRepository.LoadAllSkills().Where(x => skillIds.Contains(x.Id.GetValueOrDefault()));
			var period = new DateTimePeriod(startDateTime, endDateTime);

			var resources = _skillCombinationResourceRepository.LoadSkillCombinationResources(period).ToList();
			var intervals = _skillStaffingIntervalProvider.GetSkillStaffIntervalsAllSkills(period, resources, false);  // do we need to consider shrinkage?!

			var overTimeModels = new List<OverTimeModel>();
			//börja med skill som har öppet längst på längsta workload
			foreach (var skill in skills)
			{
				var act = skill.Activity;
				foreach (var person in persons)
				{
					var personModel = personsModels.First(x => x.PersonId == person.Id.GetValueOrDefault());

					var skillIntervals = intervals.Where(x => x.SkillId == skill.Id.GetValueOrDefault() && x.EndDateTime > personModel.End && x.StartDateTime < endDateTime).OrderBy(x => x.StartDateTime);
					if (!skillIntervals.Any()) continue;

					var shiftStart = skillIntervals.First().StartDateTime;
					var shiftEnd = shiftStart;
					var ts = skillIntervals.First().GetTimeSpan();
					foreach (var skillStaffingInterval in skillIntervals)
					{
						if (skillStaffingInterval.AbsoluteDifference >= 0) break;
						shiftEnd = shiftEnd.Add(ts);
					}
					if (shiftEnd == shiftStart) continue;

					var personSkills = person.Period(new DateOnly(startDateTime)).PersonSkillCollection.Select(x => x.Skill);
					if (!personSkills.Select(x => x.Id).Contains(skill.Id.GetValueOrDefault())) continue;

					var skillsForActivity = personSkills.Where(x => x.Activity == act).Select(y => y.Id.GetValueOrDefault());
					
					var relevantResources = resources.Where(x => x.SkillCombination.NonSequenceEquals(skillsForActivity) && x.EndDateTime > shiftStart && x.StartDateTime < shiftEnd);

					var deltas = new List<SkillCombinationResource>();
					//assume whole resource
					foreach (var resource in relevantResources)
					{
						resource.Resource += 1;
						deltas.Add(new SkillCombinationResource
								   {
									   Resource = 1,
									   StartDateTime = resource.StartDateTime,
									   EndDateTime = resource.EndDateTime,
									   SkillCombination = resource.SkillCombination
								   });
					}
					overTimeModels.Add(new OverTimeModel
									   {
										   ActivityId = act.Id.GetValueOrDefault(),
										   PersonId = person.Id.GetValueOrDefault(),
										   StartDateTime = shiftStart,
										   EndDateTime = shiftEnd,
										   Deltas = deltas
									   });

					intervals = _skillStaffingIntervalProvider.GetSkillStaffIntervalsAllSkills(period, resources, false);  // do we need to consider shrinkage??

				}
			}
			// filter persons smart when demand is filled on A and there is still demand on B
			// so we just try on add overtime that has Skill B

			return new OverTimeStaffingSuggestionModel
			{
				OverTimeModels = overTimeModels,
				SkillStaffingIntervals = intervals.Where(x => skillIds.Contains(x.SkillId)).ToList()
			};
		}
	}
	public class SkillIntervalsForOvertime
	{
		public Guid SkillId { get; set; }
		public DateTime Time { get; set; }
		public double NewStaffing { get; set; }
	}
}