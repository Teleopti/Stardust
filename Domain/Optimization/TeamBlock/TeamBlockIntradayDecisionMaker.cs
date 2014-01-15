using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ITeamBlockIntradayDecisionMaker
	{
		IList<ITeamBlockInfo> Decide(IList<ITeamBlockInfo> originalTeamBlocks,
		                                             IOptimizationPreferences optimizationPreferences,
		                                             ISchedulingOptions schedulingOptions);

		ITeamBlockInfo RecalculateTeamBlock(ITeamBlockInfo teamBlock,
		                                                    IOptimizationPreferences optimizationPreferences,
		                                                    ISchedulingOptions schedulingOptions);
	}

	public class TeamBlockIntradayDecisionMaker : ITeamBlockIntradayDecisionMaker
	{
		private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;

		public TeamBlockIntradayDecisionMaker(IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider)
		{
			_scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
		}

		public IList<ITeamBlockInfo> Decide(IList<ITeamBlockInfo> originalTeamBlocks,
		                                    IOptimizationPreferences optimizationPreferences,
		                                    ISchedulingOptions schedulingOptions)
		{
			var sortedTeamBlocks = new List<ITeamBlockInfo>();
			sortedTeamBlocks.AddRange(
				originalTeamBlocks.OrderByDescending(
					x => RecalculateTeamBlock(x, optimizationPreferences, schedulingOptions).BlockInfo.AverageOfStandardDeviations));
			sortedTeamBlocks = shuffle(sortedTeamBlocks);
			return sortedTeamBlocks;
		}

		private static List<ITeamBlockInfo> shuffle(IEnumerable<ITeamBlockInfo> teamBlocks)
		{
			var shuffledList = new List<ITeamBlockInfo>();
			var teamBockDict = new Dictionary<double, IList<ITeamBlockInfo>>();
			foreach (var teamBlockInfo in teamBlocks)
			{
				var teamBlockValue = teamBlockInfo.BlockInfo.AverageOfStandardDeviations;
				if (teamBockDict.ContainsKey(teamBlockValue))
				{
					var teamBlockWithSameValue = teamBockDict[teamBlockValue];
					teamBlockWithSameValue.Add(teamBlockInfo);
				}
				else
				{
					teamBockDict.Add(teamBlockValue, new List<ITeamBlockInfo> {teamBlockInfo});
				}
			}
			foreach (var teamBlock in teamBockDict.OrderByDescending(x => x.Key))
			{
				shuffledList.AddRange(teamBlock.Value.Randomize());
			}

			return shuffledList;
		}
		
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ITeamBlockInfo RecalculateTeamBlock(ITeamBlockInfo teamBlock,
		                                           IOptimizationPreferences optimizationPreferences,
		                                           ISchedulingOptions schedulingOptions)
		{
            var standardDeviationData = new StandardDeviationData();

			foreach (var matrix in teamBlock.TeamInfo.MatrixesForGroup())
			{
				var scheduleResultDataExtractor =
					_scheduleResultDataExtractorProvider.CreateRelativeDailyStandardDeviationsByAllSkillsExtractor(matrix,
					                                                                                               schedulingOptions);
				var values = scheduleResultDataExtractor.Values();
				var periodDays = matrix.EffectivePeriodDays;
				for (var i = 0; i < periodDays.Count; i++)
				{
					if (!standardDeviationData.Data.ContainsKey(periodDays[i].Day))
						standardDeviationData.Add(periodDays[i].Day, values[i]);
				}
			}

			var valuesOfOneBlock = new List<double?>();
			foreach (var blockDay in teamBlock.BlockInfo.BlockPeriod.DayCollection())
			{
				if (!standardDeviationData.Data.ContainsKey(blockDay)) continue;
				var value = standardDeviationData.Data[blockDay];
				valuesOfOneBlock.Add(value);
			}
			teamBlock.BlockInfo.StandardDeviations = valuesOfOneBlock;
			return teamBlock;
		}
	}
}