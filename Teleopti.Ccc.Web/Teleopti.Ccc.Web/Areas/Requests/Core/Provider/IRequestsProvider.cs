using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.Provider
{
	public interface IRequestsProvider
	{
		IEnumerable<IPersonRequest> RetrieveAbsenceAndTextRequests(RequestFilter filter, out int totalCount);

		IEnumerable<IPersonRequest> RetrieveShiftTradeRequests(RequestFilter filter, out int totalCount);

		IEnumerable<IPersonRequest> RetrieveOvertimeRequests(RequestFilter filter, out int totalCount);
	}
}