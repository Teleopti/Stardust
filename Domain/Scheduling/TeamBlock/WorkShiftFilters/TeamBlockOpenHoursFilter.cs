using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface ITeamBlockOpenHoursFilter
	{
		IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftProjectionCaches, ITeamBlockInfo teamBlockInfo, IWorkShiftFinderResult finderResult);
	}

	public class TeamBlockOpenHoursFilter : ITeamBlockOpenHoursFilter
	{
		private readonly ICreateSkillIntervalDataPerDateAndActivity _createSkillIntervalDataPerDateAndActivity;
		private readonly ISkillIntervalDataOpenHour _skillIntervalDataOpenHour;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

		public TeamBlockOpenHoursFilter(ICreateSkillIntervalDataPerDateAndActivity createSkillIntervalDataPerDateAndActivity, ISkillIntervalDataOpenHour skillIntervalDataOpenHour, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_createSkillIntervalDataPerDateAndActivity = createSkillIntervalDataPerDateAndActivity;
			_skillIntervalDataOpenHour = skillIntervalDataOpenHour;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftProjectionCaches, ITeamBlockInfo teamBlockInfo, IWorkShiftFinderResult finderResult)
		{
			if (shiftProjectionCaches == null) return null;
			if (finderResult == null) return null;
			var before = shiftProjectionCaches.Count;

			var result = new List<IShiftProjectionCache>();	
			var dayIntervalDataPerDateAndActivity = _createSkillIntervalDataPerDateAndActivity.CreateFor(teamBlockInfo, _schedulingResultStateHolder);

			foreach (var shiftProjectionCache in shiftProjectionCaches)
			{
				var mainShiftProjection = shiftProjectionCache.MainShiftProjection;
				var insideOpenHours = true;

				foreach (var visualLayer in mainShiftProjection)
				{
					var activity = visualLayer.Payload as IActivity;
					if (activity == null || !activity.RequiresSkill) continue;
					var period = visualLayer.Period;
					var start = period.StartDateTime.TimeOfDay;
					var end = period.EndDateTime.TimeOfDay;
					var timePeriod = new TimePeriod(start, end);
						
					foreach (var dateOnly in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
					{
						var dictionary = dayIntervalDataPerDateAndActivity[dateOnly];
						var openPeriod = _skillIntervalDataOpenHour.GetOpenHours(dictionary[activity], dateOnly);

						if (openPeriod.HasValue && openPeriod.Value.Contains(timePeriod)) continue;
						insideOpenHours = false;
						break;
					}

					if (!insideOpenHours) break;
				}
				
		
				if(insideOpenHours) result.Add(shiftProjectionCache);
			}

			finderResult.AddFilterResults(new WorkShiftFilterResult(string.Concat(UserTexts.Resources.FilterOnOpenHours, " "), before, result.Count));
			return result;
		}
	}
}
