using System.Collections.Generic;
using System.Linq;
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
			return sortedTeamBlocks;
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