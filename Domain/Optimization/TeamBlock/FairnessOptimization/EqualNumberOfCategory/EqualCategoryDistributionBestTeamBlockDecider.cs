using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IEqualCategoryDistributionBestTeamBlockDecider
	{
		ITeamBlockInfo FindBestSwap(ITeamBlockInfo teamBlockToSwap, IList<ITeamBlockInfo> possibleSwaps, IDistributionSummary totalDistribution, IScheduleDictionary scheduleDictionary);
	}

	public class EqualCategoryDistributionBestTeamBlockDecider : IEqualCategoryDistributionBestTeamBlockDecider
	{
		private readonly IDistributionForPersons _distributionForPersons;

		public EqualCategoryDistributionBestTeamBlockDecider(IDistributionForPersons distributionForPersons)
		{
			_distributionForPersons = distributionForPersons;
		}

		public ITeamBlockInfo FindBestSwap(ITeamBlockInfo teamBlockToSwap, IList<ITeamBlockInfo> possibleSwaps,
		                                   IDistributionSummary totalDistribution, IScheduleDictionary scheduleDictionary)
		{
			var distributionToWorkWith = _distributionForPersons.CreateSummary(teamBlockToSwap.TeamInfo.GroupMembers,
			                                                                   scheduleDictionary);

			var diffValues = new Dictionary<IShiftCategory, double>();
			foreach (var shiftCategory in totalDistribution.PercentDicionary.Keys)
			{
				var myValue = 0d;
				if (distributionToWorkWith.PercentDicionary.ContainsKey(shiftCategory))
					myValue = distributionToWorkWith.PercentDicionary[shiftCategory];

				var allValue = totalDistribution.PercentDicionary[shiftCategory];
				diffValues.Add(shiftCategory, allValue - myValue);
			}

			ITeamBlockInfo selectedTeamBlock = null;
			var highestValue = double.MinValue;
			foreach (var teamBlockInfo in possibleSwaps)
			{
				var teamBlockValue = calculateTeamBlockValue(scheduleDictionary, teamBlockInfo, diffValues);
				if (teamBlockValue.HasValue && teamBlockValue.Value > highestValue)
				{
					selectedTeamBlock = teamBlockInfo;
					highestValue = teamBlockValue.Value;
				}
			}

			return selectedTeamBlock;
		}

		private static double? calculateTeamBlockValue(IScheduleDictionary scheduleDictionary, ITeamBlockInfo teamBlockInfo,
		                                               Dictionary<IShiftCategory, double> diffValues)
		{
			double? teamBlockValue = null;
			var teamBlockInfoGroupMembers = teamBlockInfo.TeamInfo.GroupMembers.ToList();
			for (int i = 0; i < teamBlockInfoGroupMembers.Count(); i++)
			{
				foreach (var dateOnly in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
				{
					var person = teamBlockInfoGroupMembers[i];
					var day = scheduleDictionary[person].ScheduledDay(dateOnly);
					if (day.SignificantPartForDisplay() != SchedulePartView.MainShift)
						continue;

					var category = day.PersonAssignment().ShiftCategory;
					if (!teamBlockValue.HasValue)
						teamBlockValue = diffValues[category];
					else
					{
						teamBlockValue += diffValues[category];
					}
				}
			}

			return teamBlockValue;
		}
	}
}