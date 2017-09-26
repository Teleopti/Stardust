using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface IPersonRequestProvider
	{
		IEnumerable<IPersonRequest> RetrieveRequestsForLoggedOnUser(Paging paging, RequestListFilter filter);

		IEnumerable<IPersonRequest> RetrieveRequestsForLoggedOnUser(DateOnlyPeriod period);

		IPersonRequest RetrieveRequest(Guid id);
		IEnumerable<DateTimePeriod> RetrieveRequestPeriodsForLoggedOnUser(DateOnlyPeriod period);
	}
}