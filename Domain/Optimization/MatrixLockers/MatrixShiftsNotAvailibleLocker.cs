using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.MatrixLockers
{
	public interface IMatrixShiftsNotAvailibleLocker
	{
		void Execute(IEnumerable<IScheduleMatrixPro> scheduleMatrixList);
	}

	public class MatrixShiftsNotAvailibleLocker : IMatrixShiftsNotAvailibleLocker
	{
		public void Execute(IEnumerable<IScheduleMatrixPro> scheduleMatrixList)
		{
			foreach (var matrix in scheduleMatrixList)
			{
				var person = matrix.Person;
				foreach (var scheduleDayPro in matrix.EffectivePeriodDays)
				{
					var dateOnly = scheduleDayPro.Day;
					var period = person.Period(dateOnly);
					var bag = period?.RuleSetBag;
					if(bag == null)
						continue;

					bool foundOne = false;
					foreach (var workShiftRuleSet in bag.RuleSetCollection)
					{
						if (!workShiftRuleSet.IsValidDate(dateOnly)) 
							continue;

						foundOne = true;
						break;
					}

					if (!foundOne)
						matrix.LockDay(dateOnly);
				}
			}
		}
	}
}