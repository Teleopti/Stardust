using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IMatrixDataListCreator
	{
		IMatrixData[] Create(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions);
	}

	public class MatrixDataListCreator : IMatrixDataListCreator
	{
		private readonly IScheduleDayDataMapper _scheduleDayDataMapper;

		public MatrixDataListCreator(IScheduleDayDataMapper scheduleDayDataMapper)
		{
			_scheduleDayDataMapper = scheduleDayDataMapper;
		}

		public IMatrixData[] Create(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions)
		{
			return matrixList.Select(m =>
			{
				IMatrixData dataHolder = new MatrixData(_scheduleDayDataMapper);
				dataHolder.Store(m, schedulingOptions);
				return dataHolder;
			}).ToArray();
		}
	}
}