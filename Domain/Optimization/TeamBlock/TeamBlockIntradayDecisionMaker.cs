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
	}

	public class TeamBlockIntradayDecisionMaker : ITeamBlockIntradayDecisionMaker
	{
		private readonly ILockableBitArrayFactory _lockableBitArrayFactory;
		private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;

		public TeamBlockIntradayDecisionMaker(ILockableBitArrayFactory lockableBitArrayFactory,
		                                      IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider)
		{
			_lockableBitArrayFactory = lockableBitArrayFactory;
			_scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
		}

		public IList<ITeamBlockInfo> Decide(IList<ITeamBlockInfo> originalTeamBlocks,
		                                    IOptimizationPreferences optimizationPreferences,
		                                    ISchedulingOptions schedulingOptions)
		{
			var standardDeviationData = new StandardDeviationData();
			var sortedTeamBlocks = new List<ITeamBlockInfo>();
			foreach (var teamBlock in originalTeamBlocks)
			{
				foreach (var matrix in teamBlock.TeamInfo.MatrixesForGroup())
				{
					var scheduleResultDataExtractor =
						_scheduleResultDataExtractorProvider.CreateRelativeDailyStandardDeviationsByAllSkillsExtractor(matrix,
						                                                                                               schedulingOptions);
					var values = scheduleResultDataExtractor.Values();
					var periodDays = matrix.EffectivePeriodDays;
					for (var i = 0; i < periodDays.Count; i++)
					{
						var originalArray = _lockableBitArrayFactory.ConvertFromMatrix(optimizationPreferences.DaysOff.ConsiderWeekBefore,
						                                                               optimizationPreferences.DaysOff.ConsiderWeekAfter,
						                                                               matrix);
						if (originalArray.UnlockedIndexes.Contains(i) && !originalArray.DaysOffBitArray[i])
							if (!standardDeviationData.Data.ContainsKey(periodDays[i].Day))
								standardDeviationData.Add(periodDays[i].Day, values[i]);
					}
				}
			}

			foreach (var teamBlockInfo in originalTeamBlocks)
			{
				var valuesOfOneBlock = new List<double?>();
				foreach (var blockDay in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
				{
					if (!standardDeviationData.Data.ContainsKey(blockDay)) continue;
					var value = standardDeviationData.Data[blockDay];
					valuesOfOneBlock.Add(value);
				}
				teamBlockInfo.BlockInfo.StandardDeviations = valuesOfOneBlock;
			}
			sortedTeamBlocks.AddRange(originalTeamBlocks.OrderByDescending(x => x.BlockInfo.AverageStandardDeviation));
			return sortedTeamBlocks;
		}
	}
}