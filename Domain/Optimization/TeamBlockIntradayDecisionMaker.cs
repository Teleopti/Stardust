using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface ITeamBlockIntradayDecisionMaker
	{
		IBlockInfo Decide(ILockableBitArray lockableBitArray, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, IList<IScheduleMatrixPro> allPersonMatrixList, ISchedulingOptions schedulingOptions);
	}
	
	public class TeamBlockIntradayDecisionMaker : ITeamBlockIntradayDecisionMaker
	{
		private readonly IBlockProvider _blockProvider;
		private readonly IDataExtractorValuesForMatrixes _dataExtractorValuesForMatrixes;

		public TeamBlockIntradayDecisionMaker(IBlockProvider blockProvider, IDataExtractorValuesForMatrixes dataExtractorValuesForMatrixes)
		{
			_blockProvider = blockProvider;
			_dataExtractorValuesForMatrixes = dataExtractorValuesForMatrixes;
		}

		public IBlockInfo Decide(ILockableBitArray lockableBitArray, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, IList<IScheduleMatrixPro> allPersonMatrixList, ISchedulingOptions schedulingOptions)
		{
			var blocks = _blockProvider.Provide(selectedPeriod, selectedPersons, allPersonMatrixList, schedulingOptions);
			var sourceMatrixes = new HashSet<IScheduleMatrixPro>();
			foreach (var scheduleMatrixPro in allPersonMatrixList)
			{
				var periodDays = scheduleMatrixPro.FullWeeksPeriodDays;
				var fullPeriod = new DateOnlyPeriod(periodDays.First().Day, periodDays.Last().Day);
				if (blocks.Any(x => fullPeriod.Contains(x.BlockPeriod)))
				sourceMatrixes.Add(scheduleMatrixPro);
			}
			var standardDeviationData = new StandardDeviationData();
			foreach (var matrixPro in sourceMatrixes)
			{
                var values = _dataExtractorValuesForMatrixes.Data[matrixPro].Values();
				var periodDays = matrixPro.FullWeeksPeriodDays;
			    for (int i = 0; i < periodDays.Count; i++)
				{
                    if (lockableBitArray.UnlockedIndexes.Contains(i) && !lockableBitArray.DaysOffBitArray[i])
					standardDeviationData.Add(periodDays[i].Day, values[i]);
				}
			}

			foreach (var block in blocks)
			{
				var valuesOfOneBlock = new List<double?>();
				foreach (var day in block.BlockPeriod.DayCollection())
				{
					var value = standardDeviationData.Data[day];
					valuesOfOneBlock.Add(value);
				}
				block.StandardDeviations = valuesOfOneBlock;
			}
			var maxValue = blocks.Max(x => x.Average);
            return blocks.FirstOrDefault(x => Math.Abs(x.Average - maxValue) < float.Epsilon);
		}
	}
}
