using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Audit
{
	public interface IAuditAggregatorService
	{
		IList<AuditServiceModel> Load(Guid personId, DateTime startDate, DateTime endDate, string searchword);
	}
}