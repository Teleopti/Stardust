using System;
using System.Collections.Generic;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Absence
{
	public class MultiAbsenceRequestProcessor : IMultiAbsenceRequestProcessor
	{
		private readonly MultiAbsenceRequestsUpdater _absenceRequestUpdater;
		private readonly Func<ISchedulingResultStateHolder> _scheduleResultStateHolder;

		public MultiAbsenceRequestProcessor(MultiAbsenceRequestsUpdater absenceRequestUpdater,
			Func<ISchedulingResultStateHolder> scheduleResultStateHolder)
		{
			_absenceRequestUpdater = absenceRequestUpdater;
			_scheduleResultStateHolder = scheduleResultStateHolder;
		}

		public void ProcessAbsenceRequest(List<IPersonRequest> personRequests)
		{
			_absenceRequestUpdater.UpdateAbsenceRequest(personRequests, _scheduleResultStateHolder());
		}
	}
}