﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class ActivityIntervalDataCreator
	{
		private readonly CreateSkillIntervalDataPerDateAndActivity _createSkillIntervalDataPerDateAndActivity;
		private readonly IDayIntervalDataCalculator _dayIntervalDataCalculator;

		public ActivityIntervalDataCreator(
			CreateSkillIntervalDataPerDateAndActivity createSkillIntervalDataPerDateAndActivity,
			IDayIntervalDataCalculator dayIntervalDataCalculator)
		{
			_createSkillIntervalDataPerDateAndActivity = createSkillIntervalDataPerDateAndActivity;
			_dayIntervalDataCalculator = dayIntervalDataCalculator;
		}

		public Dictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>> CreateFor(IGroupPersonSkillAggregator groupPersonSkillAggregator, ITeamBlockInfo teamBlockInfo,
		                                                                                  DateOnly day,
																																											IEnumerable<ISkillDay> allSkillDays,
																							bool forRoleModel)
		{
			var skillIntervalDataPerDateAndActivity = _createSkillIntervalDataPerDateAndActivity.CreateFor(teamBlockInfo, allSkillDays, groupPersonSkillAggregator);
			var activities = new HashSet<IActivity>();
			foreach (var dicPerActivity in skillIntervalDataPerDateAndActivity.Values)
			{
				foreach (var activity in dicPerActivity.Keys)
				{
					activities.Add(activity);
				}
			}

			var activityInternalData = new Dictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>>();
			foreach (var activity in activities)
			{
				var dateOnlyDicForActivity = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
				foreach (var dateOnly in skillIntervalDataPerDateAndActivity.Keys)
				{
					if ((dateOnly == day || dateOnly == day.AddDays(1)) || forRoleModel)
					{
						var dateDic = skillIntervalDataPerDateAndActivity[dateOnly];
						IList<ISkillIntervalData> value;
						if (!dateDic.TryGetValue(activity, out value))
							continue;

						dateOnlyDicForActivity.Add(dateOnly, value);
					}
				}

				IDictionary<DateTime, ISkillIntervalData> dataForActivity =
					_dayIntervalDataCalculator.Calculate(dateOnlyDicForActivity, day);

				activityInternalData.Add(activity, dataForActivity);
			}
			return activityInternalData;
		}
	}
}