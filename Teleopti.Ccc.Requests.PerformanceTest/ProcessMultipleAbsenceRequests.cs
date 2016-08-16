using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Requests.PerformanceTest
{
	public class ProcessMultipleAbsenceRequests : IProcessMultipleAbsenceRequest
	{
		private readonly MultiAbsenceRequestsHandler _multiAbsenceRequestsHandler;

		public ProcessMultipleAbsenceRequests(MultiAbsenceRequestsHandler multiAbsenceRequestHandler)
		{
			_multiAbsenceRequestsHandler = multiAbsenceRequestHandler;
		}

		public void Process(NewMultiAbsenceRequestsCreatedEvent absenceRequests)
		{
			_multiAbsenceRequestsHandler.Handle(absenceRequests);
		}
	}
}