

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

		public ITeamBlockInfo FindBestSwap(ITeamBlockInfo teamBlockToSwap, IList<ITeamBlockInfo> possibleSwaps, IDistributionSummary totalDistribution, IScheduleDictionary scheduleDictionary)
		{
			var distributionToWorkWith = _distributionForPersons.CreateSummary(teamBlockToSwap.TeamInfo.GroupPerson.GroupMembers,
																			 scheduleDictionary);
			var maxDiff = 0d;
			IShiftCategory category = null;
			foreach (var d in distributionToWorkWith.PercentDicionary)
			{
				var diff = d.Value - totalDistribution.PercentDicionary[d.Key];
				if (diff > maxDiff)
				{
					maxDiff = diff;
					category = d.Key;
				}
			}

			if (category == null)
				return null;

			ITeamBlockInfo selectedTeamBlock = null;
			foreach (var teamBlockInfo in possibleSwaps)
			{
				var foundCategory = false;
				var teamBlockInfoGroupMembers = teamBlockInfo.TeamInfo.GroupPerson.GroupMembers.ToList();
				for (int i = 0; i < teamBlockInfoGroupMembers.Count(); i++)
				{
					foreach (var dateOnly in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
					{
						var person = teamBlockInfoGroupMembers[i];
						var day = scheduleDictionary[person].ScheduledDay(dateOnly);
						if (day.SignificantPartForDisplay() == SchedulePartView.MainShift &&
							day.PersonAssignment().ShiftCategory == category)
						{
							foundCategory = true;
							break;
						}
					}

					if (foundCategory)
						break;
				}
				if (!foundCategory)
				{
					selectedTeamBlock = teamBlockInfo;
					break;
				}
			}

			return selectedTeamBlock;
		}
	}
}