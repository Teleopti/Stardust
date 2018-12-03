using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
    public class MatrixUserLockLocker
    {
        private readonly Func<IGridlockManager> _gridlockManager;

        public MatrixUserLockLocker(Func<IGridlockManager> gridlockManager)
        {
            _gridlockManager = gridlockManager;
        }

        public void Execute(IEnumerable<IScheduleMatrixPro> scheduleMatrixes, DateOnlyPeriod selectedPeriod)
        {
            foreach (var matrix in scheduleMatrixes)
            {
	            setUserLockedDaysInMatrix(matrix, selectedPeriod, _gridlockManager());
            }
        }

		private static void setUserLockedDaysInMatrix(IScheduleMatrixPro matrix, DateOnlyPeriod selectedPeriod, IGridlockManager gridlockManager)
		{
			var currentPerson = matrix.Person;

			foreach (var dayPro in matrix.EffectivePeriodDays)
			{
				var day = dayPro.Day;

				if (selectedPeriod.Contains(day))
					matrix.UnlockPeriod(new DateOnlyPeriod(day, day));

				var locks = gridlockManager.Gridlocks(currentPerson, day);

				if (locks == null) continue;
				foreach (var userLock in locks)
				{
					if (userLock.Value.LockType.Equals(LockType.WriteProtected))
					{
						if (!PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule))
						{
							matrix.LockDay(day);
						}
					}
					else
					{
						matrix.LockDay(day);
					}
				}
			}
		}
    }
}
