using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface ICurrentScheduleReadModelPersister
	{
		void Invalidate(Guid personId);
		IEnumerable<Guid> GetInvalid();
		void Persist(Guid personId, int version, IEnumerable<ScheduledActivity> schedule);
	}
}