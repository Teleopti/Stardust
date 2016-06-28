using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
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
			if (!_currentScenario.Current().DefaultScenario)
			{
				return;
			}

			var personRequest = absenceRequest.Parent as IPersonRequest;

			if (personRequest == null || !personRequest.IsApproved)
			{
				return;
			}

			var scenario = _currentScenario.Current();
			var personAbsenceInCurrentScenarioExists = personRequest.PersonAbsences.Any (absence => absence.Scenario == scenario);

			if (!personAbsenceInCurrentScenarioExists)
			{
				personRequest?.Cancel(_personRequestCheckAuthorization);
			}
		}

		public void CancelAbsenceRequestsFromPersonAbsences (IEnumerable<IPersonAbsence> personAbsences)
		{

			foreach (var personAbsence in personAbsences.Where(perAbs => perAbs.PersonRequest != null))
			{
				var personRequest= personAbsence.PersonRequest;
				personRequest.PersonAbsences.Remove(personAbsence);

				var absenceRequest = personRequest.Request as IAbsenceRequest;
				if (absenceRequest != null)
				{
					CancelAbsenceRequest(absenceRequest);
				}
			}
		}
	}

	//ROBTODO: remove when Wfm_Requests_Cancel_37741 toggle is always on.
	public class AbsenceRequestCancelServiceWfmRequestsCancel37741ToggleOff : IAbsenceRequestCancelService
	{
		public void CancelAbsenceRequest (IAbsenceRequest absenceRequest)
		{
		}

		public void CancelAbsenceRequestsFromPersonAbsences (IEnumerable<IPersonAbsence> personAbsences)
		{
		}
	}

}