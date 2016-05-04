using System.Collections.Generic;
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
		private readonly IPrincipalAuthorization _principalAuthorization;

		public MatrixNotPermittedLocker(IPrincipalAuthorization principalAuthorization)
		{
			_principalAuthorization = principalAuthorization;
		}

		public void Execute(IList<IScheduleMatrixPro> matrixList)
		{
			foreach (var scheduleMatrixPro in matrixList)
			{
				foreach (var scheduleDayPro in scheduleMatrixPro.UnlockedDays)
				{
					var schedulePart = scheduleDayPro.DaySchedulePart();
					foreach (var persistableScheduleData in schedulePart.PersistableScheduleDataCollection())
					{
						var forAuthorization =
							new ScheduleRange.PersistableScheduleDataForAuthorization(persistableScheduleData);
						if (!_principalAuthorization.IsPermitted(forAuthorization.FunctionPath, forAuthorization.DateOnly, forAuthorization.Person))
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