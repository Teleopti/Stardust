using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class TeamBlockIntradayDecisionMaker
	{
		private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;

		public TeamBlockIntradayDecisionMaker(IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider, Func<ISchedulingResultStateHolder> schedulingResultStateHolder)
		{
			_scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public IList<ITeamBlockInfo> Decide(IList<ITeamBlockInfo> originalTeamBlocks,
		                                    SchedulingOptions schedulingOptions)
		{
			var sortedTeamBlocks = new List<ITeamBlockInfo>();
			var valueState = new Dictionary<IEnumerable<DateOnly>, IList<double?>>(new sameDates());
			sortedTeamBlocks.AddRange(
				originalTeamBlocks.OrderByDescending(
					x => recalculateTeamBlock(valueState, x, schedulingOptions).BlockInfo.AverageOfStandardDeviations));
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
				IList<ITeamBlockInfo> teamBlockWithSameValue;
				if (teamBockDict.TryGetValue(teamBlockValue, out teamBlockWithSameValue))
				{
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
		
		private ITeamBlockInfo recalculateTeamBlock(IDictionary<IEnumerable<DateOnly>, IList<double?>> valueState, ITeamBlockInfo teamBlock,
		                                           SchedulingOptions schedulingOptions)
		{
            var standardDeviationData = new StandardDeviationData();

			foreach (var matrix in teamBlock.TeamInfo.MatrixesForGroup())
			{
				var dates = matrix.EffectivePeriodDays.Select(x => x.Day).ToArray();
				if (!valueState.TryGetValue(dates, out IList<double?> values))
				{
					var scheduleResultDataExtractor = _scheduleResultDataExtractorProvider.CreateRelativeDailyStandardDeviationsByAllSkillsExtractor(dates, schedulingOptions, _schedulingResultStateHolder());
					values = scheduleResultDataExtractor.Values();
					valueState.Add(dates, values);
				}

				var periodDays = matrix.EffectivePeriodDays;
				for (var i = 0; i < periodDays.Length; i++)
				{
					if (!standardDeviationData.Data.ContainsKey(periodDays[i].Day))
						standardDeviationData.Add(periodDays[i].Day, values[i]);
				}
			}

			var valuesOfOneBlock = new List<double?>();
			foreach (var blockDay in teamBlock.BlockInfo.BlockPeriod.DayCollection())
			{
				double? value;
				if (!standardDeviationData.Data.TryGetValue(blockDay, out value)) continue;
				valuesOfOneBlock.Add(value);
			}
			teamBlock.BlockInfo.StandardDeviations = valuesOfOneBlock;
			return teamBlock;
		}

		private class sameDates : IEqualityComparer<IEnumerable<DateOnly>>
		{
			public bool Equals(IEnumerable<DateOnly> x, IEnumerable<DateOnly> y)
			{
				return new HashSet<DateOnly>(x).SetEquals(y);
			}

			public int GetHashCode(IEnumerable<DateOnly> obj)
			{
				return obj.Count();
			}
		}
	}
}