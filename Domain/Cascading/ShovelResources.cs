using System;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ShovelResources
	{
		private readonly Func<ISchedulerStateHolder> _stateHolder;
		private readonly AddResourcesToSubSkills _addResourcesToSubSkills;
		private readonly ReducePrimarySkillResources _reducePrimarySkillResources;
		private readonly SkillGroupPerActivityProvider _skillGroupPerActivityProvider;
		private readonly ITimeZoneGuard _timeZoneGuard;

		public ShovelResources(Func<ISchedulerStateHolder> stateHolder,
			AddResourcesToSubSkills addResourcesToSubSkills,
			ReducePrimarySkillResources reducePrimarySkillResources,
			SkillGroupPerActivityProvider skillGroupPerActivityProvider,
			ITimeZoneGuard timeZoneGuard)
		{
			_stateHolder = stateHolder;
			_addResourcesToSubSkills = addResourcesToSubSkills;
			_reducePrimarySkillResources = reducePrimarySkillResources;
			_skillGroupPerActivityProvider = skillGroupPerActivityProvider;
			_timeZoneGuard = timeZoneGuard;
		}

		public void Execute(DateOnlyPeriod period)
		{
			var cascadingSkills = _stateHolder().SchedulingResultState.CascadingSkills().ToArray();
			if (!cascadingSkills.Any())
				return;
			using (ResourceCalculationCurrent.PreserveContext())
			{
				using (new ResourceCalculationContextFactory(_stateHolder, () => new PersonSkillProvider(), _timeZoneGuard).Create(period))
				{
					foreach (var date in period.DayCollection())
					{
						var defaultResolution = cascadingSkills.First().DefaultResolution; //strange but cascading skills must have same resolution
						foreach (var activity in cascadingSkills.Select(x => x.Activity).Distinct())
						{
							foreach (var interval in date.ToDateTimePeriod(_timeZoneGuard.CurrentTimeZone()).Intervals(TimeSpan.FromMinutes(defaultResolution)))
							{
								foreach (var skillGroup in _skillGroupPerActivityProvider.FetchOrdered(activity, interval))
								{
									var resourcesMoved = _addResourcesToSubSkills.Execute(skillGroup, interval);
									_reducePrimarySkillResources.Execute(skillGroup.PrimarySkills, interval, resourcesMoved);
								}
							}
						}
					}
				}
			}
		}
	}
}