using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{

	public class SeatPlanningResult : ISeatPlanningResult
	{
		public int RequestsDenied => (NumberOfBookingRequests - RequestsGranted);
		public int NumberOfBookingRequests { get; set; }
		public int RequestsGranted { get; set; }
		public int NumberOfUnscheduledAgentDays { get; set; }
	}


	public class SeatBookingSummary 
	{
		
		public int NumberOfScheduleDaysWithoutBookings { get; set; }
		public int NumberOfBookings { get; set; }
		public int NumberOfUnscheduledAgentDays { get; set; }
	}

}