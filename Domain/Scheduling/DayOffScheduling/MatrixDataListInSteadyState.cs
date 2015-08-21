using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IMatrixDataListInSteadyState
	{
		bool IsListInSteadyState(IList<IMatrixData> matrixDataList);
	}

	public class MatrixDataListInSteadyState : IMatrixDataListInSteadyState
	{
		public bool IsListInSteadyState(IList<IMatrixData> matrixDataList)
		{
			IMatrixData masterMatrix = matrixDataList[0];
			DateOnlyPeriod masterPeriod = dateOnlyPeriodFromData(masterMatrix);
			foreach (var matrixData in matrixDataList)
			{
				if (dateOnlyPeriodFromData(matrixData) != masterPeriod)
					return false;

				if (matrixData.TargetDaysOff != masterMatrix.TargetDaysOff)
					return false;

				foreach (var dateOnly in masterPeriod.DayCollection())
				{
					if (matrixData[dateOnly].IsDayOff)
					{
						foreach (var matrixData1 in matrixDataList)
						{
							IScheduleDayData value;
							if (matrixData1.TryGetValue(dateOnly, out value))
						    {
						        if (!value.IsDayOff)
						            return false;
						    }
						    else
						    {
						        return false;
						    }
						}
					}
				}
			}

			return true;
		}

		private static DateOnlyPeriod dateOnlyPeriodFromData(IMatrixData data)
		{
			return new DateOnlyPeriod(data.ScheduleDayDataCollection.First().DateOnly, data.ScheduleDayDataCollection.Last().DateOnly);
		}
	}
}