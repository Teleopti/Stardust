using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface ISkillIntervalDataDivider
	{
		IList<ISkillIntervalData> SplitSkillIntervalData(IList<ISkillIntervalData> skillIntervalDataList, int resolution);
	}
	public class SkillIntervalDataDivider : ISkillIntervalDataDivider
	{
		public IList<ISkillIntervalData> SplitSkillIntervalData(IList<ISkillIntervalData> skillIntervalDataList, int resolution)
		{
			var resultingskillIntervalDataList = new List<ISkillIntervalData>();
			if (skillIntervalDataList != null)
			{
				var modDiffInMin = (skillIntervalDataList[0].Period.EndDateTime.Minute -
				                    skillIntervalDataList[0].Period.StartDateTime.Minute) % resolution;

				int totallDiffInMin = (skillIntervalDataList[0].Period.EndDateTime.Minute -
				                       skillIntervalDataList[0].Period.StartDateTime.Minute) / resolution;
				if (modDiffInMin != 0)
				{
					//split the interval into to 30 min as the interval length is 15 and the resolution is 10
					var sortedSkillList =
						skillIntervalDataList.OrderBy(s => s.Period.StartDateTime).ThenBy(s => s.Period.EndDateTime).ToList();
					var aggregatedList = new List<ISkillIntervalData>();
					for (var j = 0; j < sortedSkillList.Count(); j++)
					{
						if (j + 1 < skillIntervalDataList.Count)
						{
							aggregatedList.Add(aggregateTwoIntervals(sortedSkillList[j], sortedSkillList[j + 1]));
							j++;
						}
						else
							aggregatedList.Add(aggregateTwoIntervals(sortedSkillList[j], null));

					}
					skillIntervalDataList = aggregatedList;
					totallDiffInMin = (skillIntervalDataList[0].Period.EndDateTime.Minute -
					                   skillIntervalDataList[0].Period.StartDateTime.Minute) / resolution;
				}
				foreach (var skillIntervalItem in skillIntervalDataList)
				{
					var startPeriod = skillIntervalItem.Period.StartDateTime;
					for (int j = 0; j < totallDiffInMin; j++)
					{
						resultingskillIntervalDataList.Add(
							new SkillIntervalData(new DateTimePeriod(startPeriod, startPeriod.AddMinutes(resolution)),
							                      skillIntervalItem.RelativeDifference));
						startPeriod = startPeriod.AddMinutes(resolution);
					}
				}
			}


			return resultingskillIntervalDataList;
		}

		private static ISkillIntervalData aggregateTwoIntervals(ISkillIntervalData skillIntervalData1, ISkillIntervalData skillIntervalData2)
		{
			if (skillIntervalData2 == null)
			{
				return new SkillIntervalData(new DateTimePeriod(skillIntervalData1.Period.StartDateTime, skillIntervalData1.Period.EndDateTime.AddMinutes(15)),
				                             skillIntervalData1.RelativeDifference);
			}

			return new SkillIntervalData(new DateTimePeriod(skillIntervalData1.Period.StartDateTime, skillIntervalData2.Period.EndDateTime),
			                             (skillIntervalData1.RelativeDifference + skillIntervalData2.RelativeDifference) / 2);
		}
	}
}