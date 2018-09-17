﻿using System;
using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence.Domain.Service
{
	public interface ICurrentScheduleReadModelPersister
	{
		void Invalidate(Guid personId);
		IEnumerable<Guid> GetInvalid();
		void Persist(Guid personId, int revision, IEnumerable<ScheduledActivity> schedule);
	}
}