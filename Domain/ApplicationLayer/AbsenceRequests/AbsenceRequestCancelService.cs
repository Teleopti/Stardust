using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class AbsenceRequestCancelService : IAbsenceRequestCancelService
	{

		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;

		public AbsenceRequestCancelService(IPersonRequestCheckAuthorization personRequestCheckAuthorization)
		{
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
		}

		public void CancelAbsenceRequest(IAbsenceRequest absenceRequest)
		{
			var personRequest = absenceRequest.Parent as IPersonRequest;

			if (personRequest == null || !personRequest.IsApproved)
			{
				return;
			}

			if (personRequest.PersonAbsences.IsEmpty())
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