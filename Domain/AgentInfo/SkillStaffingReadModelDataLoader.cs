using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class SkillStaffingReadModelDataLoader : ISkillStaffingReadModelDataLoader
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly ExtractSkillForecastIntervals _extractSkillForecastIntervals;
		private readonly IResourceCalculation _resourceCalculation;

		public SkillStaffingReadModelDataLoader(ILoggedOnUser loggedOnUser,
			ISkillCombinationResourceRepository skillCombinationResourceRepository,
			ExtractSkillForecastIntervals extractSkillForecastIntervals, IResourceCalculation resourceCalculation)
		{
			_loggedOnUser = loggedOnUser;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_extractSkillForecastIntervals = extractSkillForecastIntervals;
			_resourceCalculation = resourceCalculation;
		}

		public IList<SkillStaffingData> Load(IList<ISkill> skills, DateTimePeriod dateTimePeriod)
		{
			var skillStaffingList = new List<SkillStaffingData>();
			if (!skills.Any()) return skillStaffingList;

			var skillStaffingDatas = createSkillStaffingDatas(dateTimePeriod, skills);

			skillStaffingList.AddRange(skillStaffingDatas);
			return skillStaffingList;
		}

		private IEnumerable<SkillStaffingData> createSkillStaffingDatas(DateTimePeriod period, IList<ISkill> skills)
		{
			var timezone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var dateOnlyPeriod = period.ToDateOnlyPeriod(timezone);
			var fullDayPeriod = dateOnlyPeriod.ToDateTimePeriod(timezone);
			var skillIds = skills.Select(skill => skill.Id).ToList();
			var combinationResources =
				_skillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(fullDayPeriod.StartDateTime.AddDays(-2), fullDayPeriod.EndDateTime.AddDays(2)))
					.Where(s => s.SkillCombination.All(skillId => skillIds.Contains(skillId)))
					.ToList();

			using (getContext(combinationResources, skills, false))
			{
				var skillStaffingIntervals = _extractSkillForecastIntervals.GetBySkills(skills, fullDayPeriod, false).ToList();
				skillStaffingIntervals.ForEach(s => s.StaffingLevel = 0);

				var relevantSkillStaffPeriods =
					skillStaffingIntervals.GroupBy(s => skills.First(a => a.Id.GetValueOrDefault() == s.SkillId))
						.ToDictionary(k => k.Key,
							v =>
								(IResourceCalculationPeriodDictionary)
								new ResourceCalculationPeriodDictionary(v.ToDictionary(d => d.DateTimePeriod,
									s => (IResourceCalculationPeriod)s)));
				var resourceCalculationData = new ResourceCalculationData(skills,
					new SlimSkillResourceCalculationPeriodWrapper(relevantSkillStaffPeriods));
				_resourceCalculation.ResourceCalculate(period.ToDateOnlyPeriod(TimeZoneInfo.Utc),
					resourceCalculationData, () => getContext(combinationResources, skills, false));


				var skillStaffingDatas = new List<SkillStaffingData>();
				var calculationPeriodDictionarys = resourceCalculationData.SkillResourceCalculationPeriodDictionary.Items();
				var resolution = skills.Min(s => s.DefaultResolution);

				foreach (var calculationPeriodDictionary in calculationPeriodDictionarys)
				{
					foreach (var resourceCalculationPeriod in calculationPeriodDictionary.Value.Items())
					{
						skillStaffingDatas.Add(new SkillStaffingData
						{
							Skill = calculationPeriodDictionary.Key,
							ForecastedStaffing = ((SkillStaffingInterval)resourceCalculationPeriod.Value).FStaff,
							ScheduledStaffing = ((SkillStaffingInterval)resourceCalculationPeriod.Value).StaffingLevel,
							Time = resourceCalculationPeriod.Key.StartDateTime,
							Date = new DateOnly(resourceCalculationPeriod.Key.StartDateTime),
							Resolution = resolution
						});
					}
				}

				return skillStaffingDatas;
			}
		}

		private static IDisposable getContext(List<SkillCombinationResource> combinationResources, IList<ISkill> skills, bool useAllSkills)
		{
			return new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => new ResourceCalculationDataContainerFromSkillCombinations(combinationResources, skills, useAllSkills)));
		}
	}
}
