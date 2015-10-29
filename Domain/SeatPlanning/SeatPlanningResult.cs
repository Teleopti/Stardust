using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatPlanningResult : ISeatPlanningResult
	{
		public int NumberOfBookingRequests { get; set; }
		public int RequestsGranted { get; set; }
		public int RequestsDenied
		{
			get
			{
				return (NumberOfBookingRequests - RequestsGranted);
			}
		}

		public int NumberOfUnscheduledAgentDays { get; set; }
	}
}