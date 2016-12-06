using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IJobStartTimeRepository
	{
		void Persist(Guid buId, DateTime datetime);
		IDictionary<Guid, DateTime> LoadAll();
	}
}