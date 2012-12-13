using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IMissingDaysOffScheduler
	{
		bool Execute(IList<IScheduleMatrixPro> matrixList);
	}

	public class MissingDaysOffScheduler : IMissingDaysOffScheduler
	{
		private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;

		public MissingDaysOffScheduler(IDayOffsInPeriodCalculator dayOffsInPeriodCalculator)
		{
			_dayOffsInPeriodCalculator = dayOffsInPeriodCalculator;
		}

		public bool Execute(IList<IScheduleMatrixPro> matrixList)
		{
			IList<IMatrixData> matrixDatas = new List<IMatrixData>();
			

			bool useSameDaysOffOnAll = allMatrixesHaveSameDaysOff(matrixList);

			//om alla är lika så jobbar vi bara med första matrixen och sedan assignar vi till alla
			//om de inte är lika så jobbar vi med var och en försig

			//en metod som hittar nästa möjliga spot på en matrix, hur ska jag kunna testa den

			IList<IScheduleMatrixPro> workingList = findMatrixesWithToFewDaysOff(matrixList);
			//while (workingList.Count > 0)
			//{
			//    //find first possible day before first contract day off that are available in all matrixes
			//    workingList = findMatrixesWithToFewDaysOff(workingList);
			//}

			return true;
		}

		private IList<IScheduleMatrixPro> findMatrixesWithToFewDaysOff(IList<IScheduleMatrixPro> matrixList)
		{
			IList<IScheduleMatrixPro> result = new List<IScheduleMatrixPro>();
			//foreach (var matrix in matrixList)
			//{
			//    int targetDaysOff;
			//    int daysOffNow;
			//    if(!_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(matrix.SchedulePeriod, out targetDaysOff, out daysOffNow))
			//        result.Add(matrix);
			//}

			return result;
		}

		private bool allMatrixesHaveSameDaysOff(IList<IScheduleMatrixPro> matrixList)
		{
			//IScheduleMatrixPro masterMatrix = matrixList[0];
			//DateOnlyPeriod masterPeriod = masterMatrix.SchedulePeriod.DateOnlyPeriod;
			//foreach (var matrix in matrixList)
			//{
			//    if (matrix.SchedulePeriod.DateOnlyPeriod != masterPeriod)
			//        return false;

			//    foreach (var dateOnly in matrix.SchedulePeriod.DateOnlyPeriod.DayCollection())
			//    {
			//        if(matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart().SignificantPart()==SchedulePartView.DayOff)
			//        {
			//            foreach (var matrixPro in matrixList)
			//            {
			//                if (matrixPro.GetScheduleDayByKey(dateOnly).DaySchedulePart().SignificantPart() != SchedulePartView.DayOff)
			//                    return false;
			//            }
			//        }
			//    }
			//}

			return true;
		}
	}
}