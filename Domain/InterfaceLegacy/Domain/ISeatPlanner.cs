using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ISeatPlanningResult
	{
		int NumberOfBookingRequests { get; set; }
		int RequestsGranted { get; set; }
		int RequestsDenied { get; }
		int NumberOfUnscheduledAgentDays { get; set; }
	}
	
	public interface ISeatPlanner
	{
		ISeatPlanningResult Plan (IList<Guid> locationIds, IList<Guid> teamIds, DateOnlyPeriod dateOnlyPeriod, List<Guid> seatIds, List<Guid> personIds);
	}
}