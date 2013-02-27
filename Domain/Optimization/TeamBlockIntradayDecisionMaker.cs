using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface ITeamBlockIntradayDecisionMaker
	{
		IIntradayBlock Decide(IList<IScheduleMatrixPro> selectedPersonMatrixList, ISchedulingOptions schedulingOptions);
	}
	
	public class TeamBlockIntradayDecisionMaker : ITeamBlockIntradayDecisionMaker
	{
		private readonly IBlockProvider _blockProvider;
		private readonly ILockableData _lockableData;

		public TeamBlockIntradayDecisionMaker(IBlockProvider blockProvider, ILockableData lockableData)
		{
			_blockProvider = blockProvider;
			_lockableData = lockableData;
		}

		public IIntradayBlock Decide(IList<IScheduleMatrixPro> selectedPersonMatrixList, ISchedulingOptions schedulingOptions)
		{
			var blocks = _blockProvider.Provide(selectedPersonMatrixList);
			var sourceMatrixes = new HashSet<IScheduleMatrixPro>();
			foreach (var scheduleMatrixPro in selectedPersonMatrixList)
			{
				var periodDays = scheduleMatrixPro.FullWeeksPeriodDays;
				var fullPeriod = new DateOnlyPeriod(periodDays.First().Day, periodDays.Last().Day);
				if (blocks.Any(x => fullPeriod.Contains(x.CoveringPeriod)))
				sourceMatrixes.Add(scheduleMatrixPro);
			}
			var standardDeviationData = new StandardDeviationData();
			foreach (var matrixPro in sourceMatrixes)
			{
				var values = _lockableData.Data[matrixPro].DataExtractor.Values();
				var periodDays = matrixPro.FullWeeksPeriodDays;
				for (int i = 0; i < periodDays.Count; i++)
				{
					standardDeviationData.Add(periodDays[i].Day, values[i]);
				}
			}

			foreach (var block in blocks)
			{
				var valuesOfOneBlock = new List<double?>();
				foreach (var day in block.BlockDays)
				{
					var value = standardDeviationData.Data[day];
					valuesOfOneBlock.Add(value);
				}
				block.StandardDeviations = valuesOfOneBlock;
			}
			var maxValue = blocks.Max(x => x.Sum);
			return blocks.FirstOrDefault(x=>x.Sum == maxValue);
		}
	}
}
