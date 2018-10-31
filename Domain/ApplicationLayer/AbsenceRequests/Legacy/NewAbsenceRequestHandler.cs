using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.Legacy
{
	public class NewAbsenceRequestHandler
	{
		private readonly IAbsenceRequestProcessor _absenceRequestProcessor;

		public NewAbsenceRequestHandler(IAbsenceRequestProcessor absenceRequestProcessor)
		{
			_absenceRequestProcessor = absenceRequestProcessor;
		}

		public IPersonRequest Handle(IPersonRequest personRequest)
		{
			if (!personRequest.IsDenied)
			{
				_absenceRequestProcessor.Process(personRequest);
			}

			return personRequest;
		}
	}
}