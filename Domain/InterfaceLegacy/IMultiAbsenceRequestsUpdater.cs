using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy
{
	public interface IMultiAbsenceRequestsUpdater
	{
		void UpdateAbsenceRequest(IList<Guid> personRequests);

		void UpdateAbsenceRequest(IList<Guid> personRequests, IDictionary<Guid, IEnumerable<IAbsenceRequestValidator>> absenceRequestValidators);
	}
}