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
        IBlockInfo Decide(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, IList<IScheduleMatrixPro> allPersonMatrixList, IDataExtractorValuesForMatrixes dataExtractorValuesForMatrixes,
            ISchedulingOptions schedulingOptions,
            IOptimizationPreferences optimizationPreferences);
	}
	
	public class TeamBlockIntradayDecisionMaker : ITeamBlockIntradayDecisionMaker
	{
	    private readonly ILockableBitArrayFactory _lockableBitArrayFactory;
	    private readonly IBlockProvider _blockProvider;

	    public TeamBlockIntradayDecisionMaker(ILockableBitArrayFactory lockableBitArrayFactory,
            IBlockProvider blockProvider
           
            )
		{
            _lockableBitArrayFactory = lockableBitArrayFactory;
            _blockProvider = blockProvider;
		}

        public IBlockInfo Decide(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, IList<IScheduleMatrixPro> allPersonMatrixList, IDataExtractorValuesForMatrixes dataExtractorValuesForMatrixes,
            ISchedulingOptions schedulingOptions,
            IOptimizationPreferences optimizationPreferences)
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
                var values = dataExtractorValuesForMatrixes.Data[matrixPro].Values();
                var periodDays = matrixPro.EffectivePeriodDays;
                for (int i = 0; i < periodDays.Count; i++)
                {
                    ILockableBitArray originalArray =
                      _lockableBitArrayFactory.ConvertFromMatrix(optimizationPreferences.DaysOff.ConsiderWeekBefore,
                                                                 optimizationPreferences.DaysOff.ConsiderWeekAfter,
                                                                 matrixPro);
                    if (originalArray.UnlockedIndexes.Contains(i) && !originalArray.DaysOffBitArray[i])
                        if (!standardDeviationData.Data.ContainsKey(periodDays[i].Day))
                            standardDeviationData.Add(periodDays[i].Day, values[i]);
                }
            }

            foreach (var block in blocks)
            {
                var valuesOfOneBlock = new List<double?>();
                foreach (var day in block.BlockPeriod.DayCollection())
                {
                    if (!standardDeviationData.Data.ContainsKey(day)) continue;
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
