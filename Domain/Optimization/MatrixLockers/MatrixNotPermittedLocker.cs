﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Optimization.MatrixLockers
{
	public class MatrixNotPermittedLocker
	{
		private readonly ICurrentAuthorization _authorization;

		public MatrixNotPermittedLocker(ICurrentAuthorization authorization)
		{
			_authorization = authorization;
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
							new PersistableScheduleDataForAuthorization(persistableScheduleData);
						if (!_authorization.Current().IsPermitted(forAuthorization.FunctionPath, forAuthorization.DateOnly, forAuthorization.Person))
						{
							scheduleMatrixPro.LockDay(scheduleDayPro.Day);
							break;
						}
					}
				}
			}
		}
	}
}