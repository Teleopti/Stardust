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
		private readonly IScheduleDayDataMapper _scheduleDayDataMapper;

		public MatrixDataListCreator(IScheduleDayDataMapper scheduleDayDataMapper)
		{
			_scheduleDayDataMapper = scheduleDayDataMapper;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IList<IMatrixData> Create(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions)
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