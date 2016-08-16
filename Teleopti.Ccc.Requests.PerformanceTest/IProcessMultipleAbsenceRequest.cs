using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Requests.PerformanceTest
{
	public interface IProcessMultipleAbsenceRequest
	{
		void Process(NewMultiAbsenceRequestsCreatedEvent absenceRequests);
	}
}