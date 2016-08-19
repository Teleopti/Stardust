using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class AbsenceRequestCancelService : IAbsenceRequestCancelService
	{
		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;
		private readonly ICurrentScenario _currentScenario;

		public AbsenceRequestCancelService(IPersonRequestCheckAuthorization personRequestCheckAuthorization, ICurrentScenario currentScenario)
		{
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
			_currentScenario = currentScenario;
		}

		public void CancelAbsenceRequest(IAbsenceRequest absenceRequest)
		{
			// only allow cancel if this is the default scenario.
			var currentScenario = _currentScenario.Current();
			if (!currentScenario.DefaultScenario)
			{
				return;
			}

			var personRequest = absenceRequest.Parent as IPersonRequest;
			if (personRequest == null || !personRequest.IsApproved)
			{
				return;
			}

			var personAbsenceInCurrentScenarioExists =
				personRequest.PersonAbsences.Any(absence => absence.Scenario == currentScenario);
			if (!personAbsenceInCurrentScenarioExists)
			{
				personRequest.Cancel(_personRequestCheckAuthorization);
			}
		}

		public void CancelAbsenceRequestsFromPersonAbsence(IPersonAbsence personAbsence)
		{
			if (personAbsence.PersonRequest == null) return;
			var personRequest = personAbsence.PersonRequest;
			personRequest.PersonAbsences.Remove(personAbsence);

			var absenceRequest = personRequest.Request as IAbsenceRequest;
			if (absenceRequest != null)
			{
				CancelAbsenceRequest(absenceRequest);
			}
		}
	}

	//ROBTODO: remove when Wfm_Requests_Cancel_37741 toggle is always on.
	public class AbsenceRequestCancelServiceWfmRequestsCancel37741ToggleOff : IAbsenceRequestCancelService
	{
		public void CancelAbsenceRequest (IAbsenceRequest absenceRequest)
		{
		}

		public void CancelAbsenceRequestsFromPersonAbsence(IPersonAbsence personAbsence)
		{
		}
	}
}