

using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IActivityIntervalDataCreator
	{
		Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> CreateFor(ITeamBlockInfo teamBlockInfo,
		                                                                                           DateOnly day,
		                                                                                           ISchedulingResultStateHolder
			                                                                                           schedulingResultStateHolder);
	}

	public class ActivityIntervalDataCreator : IActivityIntervalDataCreator
	{
		private readonly ICreateSkillIntervalDataPerDateAndActivity _createSkillIntervalDataPerDateAndActivity;
		private readonly IDayIntervalDataCalculator _dayIntervalDataCalculator;

		public ActivityIntervalDataCreator(
			ICreateSkillIntervalDataPerDateAndActivity createSkillIntervalDataPerDateAndActivity,
			IDayIntervalDataCalculator dayIntervalDataCalculator)
		{
			_createSkillIntervalDataPerDateAndActivity = createSkillIntervalDataPerDateAndActivity;
			_dayIntervalDataCalculator = dayIntervalDataCalculator;
		}

		public Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> CreateFor(ITeamBlockInfo teamBlockInfo,
		                                                                                  DateOnly day,
		                                                                                  ISchedulingResultStateHolder
			                                                                                  schedulingResultStateHolder)
		{
			var skillIntervalDataPerDateAndActivity = _createSkillIntervalDataPerDateAndActivity.CreateFor(teamBlockInfo,
			                                                                                               schedulingResultStateHolder);
			var activities = new HashSet<IActivity>();
			foreach (var dicPerActivity in skillIntervalDataPerDateAndActivity.Values)
			{
				foreach (var activity in dicPerActivity.Keys)
				{
					activities.Add(activity);
				}
			}

			var activityInternalData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();
			foreach (var activity in activities)
			{
				var dateOnlyDicForActivity = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
				foreach (var dateOnly in skillIntervalDataPerDateAndActivity.Keys)
				{
					if (dateOnly == day || dateOnly == day.AddDays(1))
					{
						var dateDic = skillIntervalDataPerDateAndActivity[dateOnly];
						if (!dateDic.ContainsKey(activity))
							continue;

						dateOnlyDicForActivity.Add(dateOnly, dateDic[activity]);
					}
				}

				IDictionary<TimeSpan, ISkillIntervalData> dataForActivity =
					_dayIntervalDataCalculator.Calculate(dateOnlyDicForActivity);

				if (dataForActivity.Any())
					activityInternalData.Add(activity, dataForActivity);
			}
			return activityInternalData;
		}
	}
}