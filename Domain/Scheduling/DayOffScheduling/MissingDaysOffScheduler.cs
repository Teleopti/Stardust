using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IMissingDaysOffScheduler
	{
		bool Execute(IList<IMatrixData> matrixDataList);
	}

	public class MissingDaysOffScheduler : IMissingDaysOffScheduler
	{
		private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		private readonly IBestSpotForAddingDayOffFinder _bestSpotForAddingDayOffFinder;

		public MissingDaysOffScheduler(IDayOffsInPeriodCalculator dayOffsInPeriodCalculator, IBestSpotForAddingDayOffFinder bestSpotForAddingDayOffFinder)
		{
			_dayOffsInPeriodCalculator = dayOffsInPeriodCalculator;
			_bestSpotForAddingDayOffFinder = bestSpotForAddingDayOffFinder;
		}

		public bool Execute(IList<IMatrixData> matrixDataList)
		{
			bool useSameDaysOffOnAll = true;// = allMatrixesHaveSameDaysOff(matrixDataList);
			IList<IMatrixData> workingList = findMatrixesWithToFewDaysOff(matrixDataList);
			while (workingList.Count > 0)
			{
				if(useSameDaysOffOnAll)
				{
					DateOnly? resultingDate = _bestSpotForAddingDayOffFinder.Find(workingList[0].ScheduleDayDataCollection);
					if(!resultingDate.HasValue)
					{
						//rollback
						return false;
					}
				}
				//find first possible day before first contract day off that are available in all matrixes
				workingList = findMatrixesWithToFewDaysOff(workingList);
			}


			//om alla är lika så jobbar vi bara med första matrixen och sedan assignar vi till alla
			//om de inte är lika så jobbar vi med var och en försig

			//en metod som hittar nästa möjliga spot på en matrix, hur ska jag kunna testa den

			

			return true;
		}

		private IList<IMatrixData> findMatrixesWithToFewDaysOff(IList<IMatrixData> matrixDataList)
		{
			IList<IMatrixData> result = new List<IMatrixData>();
			foreach (var matrixData in matrixDataList)
			{
				int targetDaysOff;
				int daysOffNow;
				if (!_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(matrixData.Matrix.SchedulePeriod, out targetDaysOff, out daysOffNow))
					result.Add(matrixData);
			}

			return result;
		}

		
	}
}