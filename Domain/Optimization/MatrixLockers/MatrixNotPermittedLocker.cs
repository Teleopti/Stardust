

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.MatrixLockers
{
	public interface IMatrixNotPermittedLocker
	{
		void Execute(IList<IScheduleMatrixPro> matrixList);
	}

	public class MatrixNotPermittedLocker : IMatrixNotPermittedLocker
	{
		public void Execute(IList<IScheduleMatrixPro> matrixList)
		{
			var authorization = PrincipalAuthorization.Instance();
			foreach (var scheduleMatrixPro in matrixList)
			{
				foreach (var scheduleDayPro in scheduleMatrixPro.UnlockedDays)
				{
					var schedulePart = scheduleDayPro.DaySchedulePart();
					foreach (var persistableScheduleData in schedulePart.PersistableScheduleDataCollection())
					{
						var forAuthorization =
							new ScheduleRange.PersistableScheduleDataForAuthorization(persistableScheduleData);
						if (!authorization.IsPermitted(forAuthorization.FunctionPath, forAuthorization.DateOnly, forAuthorization.Person))
						{
							scheduleMatrixPro.LockPeriod(new DateOnlyPeriod(scheduleDayPro.Day, scheduleDayPro.Day));
							break;
						}
					}
				}
			}
		}
	}
}