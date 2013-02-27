using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface ILockableDataProvider
	{
		ILockableData Provide();
	}

	public class LockableDataProvider : ILockableDataProvider
	{
		private readonly IList<IScheduleMatrixPro> _matrixes;
		private readonly IOptimizationPreferences _optimizationPreferences;

		public LockableDataProvider(IList<IScheduleMatrixPro> matrixes, IOptimizationPreferences optimizationPreferences)
		{
			_matrixes = matrixes;
			_optimizationPreferences = optimizationPreferences;
		}

		public ILockableData Provide()
		{
			var matrixesData = new LockableData();
			foreach (var scheduleMatrixPro in _matrixes)
			{
				var converter = new ScheduleMatrixLockableBitArrayConverter(scheduleMatrixPro);
				var extractor = new RelativeDailyValueByPersonalSkillsExtractor(scheduleMatrixPro, _optimizationPreferences.Advanced);
				matrixesData.Add(scheduleMatrixPro, new IntradayDecisionMakerComponents(converter, extractor));
			}
			return matrixesData;
		}
	}
}