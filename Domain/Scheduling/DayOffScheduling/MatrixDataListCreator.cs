using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IMatrixDataListCreator
	{
		IList<IMatrixData> Create(IEnumerable<IScheduleMatrixPro> matrixList, SchedulingOptions schedulingOptions);
	}

	public class MatrixDataListCreator : IMatrixDataListCreator
	{
		private readonly IScheduleDayDataMapper _scheduleDayDataMapper;

		public MatrixDataListCreator(IScheduleDayDataMapper scheduleDayDataMapper)
		{
			_scheduleDayDataMapper = scheduleDayDataMapper;
		}

		public IList<IMatrixData> Create(IEnumerable<IScheduleMatrixPro> matrixList, SchedulingOptions schedulingOptions)
		{
			IList<IMatrixData> result = new List<IMatrixData>();
			foreach (var scheduleMatrixPro in matrixList)
			{
				IMatrixData dataHolder = new MatrixData(_scheduleDayDataMapper);
				dataHolder.Store(scheduleMatrixPro, schedulingOptions);
				result.Add(dataHolder);
			}

			return result;
		}
	}
}