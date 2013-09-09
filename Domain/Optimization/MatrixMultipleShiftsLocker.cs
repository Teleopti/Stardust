using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IMatrixMultipleShiftsLocker
	{
		void Execute();
	}

	public class MatrixMultipleShiftsLocker : IMatrixMultipleShiftsLocker
	{
		private readonly IList<IScheduleMatrixPro> _matrixList;

		public MatrixMultipleShiftsLocker(IList<IScheduleMatrixPro> matrixList)
		{
			_matrixList = matrixList;
		}

		public void Execute()
		{
			foreach (var scheduleMatrix in _matrixList)
			{
				foreach (var day in scheduleMatrix.EffectivePeriodDays)
				{
					var scheduleDay = day.DaySchedulePart();
					var dateOnly = day.Day;
					
					if (scheduleDay.PersonAssignmentCollection().Count > 1)
					{
						scheduleMatrix.LockPeriod(new DateOnlyPeriod(dateOnly, dateOnly));
					}
				}
			}
		}
	}
}
