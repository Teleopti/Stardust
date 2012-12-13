using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IMatrixDataListCreator
	{
		IList<IMatrixData> Create(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions);
	}

	public class MatrixDataListCreator : IMatrixDataListCreator
	{
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;

		public MatrixDataListCreator(IEffectiveRestrictionCreator effectiveRestrictionCreator)
		{
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
		}

		public IList<IMatrixData> Create(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions)
		{
			IList<IMatrixData> result = new List<IMatrixData>();
			foreach (var scheduleMatrixPro in matrixList)
			{
				result.Add(new MatrixData(scheduleMatrixPro, _effectiveRestrictionCreator, schedulingOptions));
			}

			return result;
		}
	}
}