using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class ResourceAllocator
	{
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;

		public ResourceAllocator(IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository)
		{
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
		}

		public IEnumerable<StaffingIntervalChange> AllocateResource(IPersonRequest personRequest)
		{
			var ret = new List<StaffingIntervalChange> ();

			var cascadingPersonSkills = personRequest.Person.Period(DateOnly.Today).CascadingSkills();
			var lowestIndex = cascadingPersonSkills.Min(x => x.Skill.CascadingIndex);

			var skills = personRequest.Person.Period(DateOnly.Today).PersonSkillCollection.Select(x => x.Skill).Where(y => !y.IsCascading() || y.CascadingIndex == lowestIndex);

			var skillStaffingIntervals = new List<SkillStaffingInterval>();
			foreach (var skill in skills)
			{
				var intervals = _scheduleForecastSkillReadModelRepository.GetBySkill(skill.Id.GetValueOrDefault(), personRequest.Request.Period.StartDateTime, personRequest.Request.Period.EndDateTime).ToList();
				skillStaffingIntervals.AddRange(intervals);
			}

			foreach (var startDate in skillStaffingIntervals.GroupBy(x => x.StartDateTime))
			{
				var intervalsWithSamePeriod = skillStaffingIntervals.Where(x => x.StartDateTime == startDate.Key);

				double totaloverstaffing = intervalsWithSamePeriod.Sum(interval => interval.StaffingLevel - interval.ForecastWithShrinkage);

				foreach (var interval in intervalsWithSamePeriod)
				{
					var staffingIntervalChange = new StaffingIntervalChange()
					{
						StartDateTime = interval.StartDateTime,
						EndDateTime = interval.EndDateTime,
						SkillId = interval.SkillId,
						StaffingLevel = -(interval.StaffingLevel - interval.ForecastWithShrinkage) / totaloverstaffing
					};
					ret.Add(staffingIntervalChange);
					
				}
			}

			return ret;
		}
	}
}