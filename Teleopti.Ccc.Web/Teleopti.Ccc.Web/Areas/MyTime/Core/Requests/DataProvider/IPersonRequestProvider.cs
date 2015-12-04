using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface IPersonRequestProvider
	{
		IEnumerable<IPersonRequest> RetrieveRequestsForLoggedOnUser(Paging paging);
		IEnumerable<IPersonRequest> RetrieveRequestsForLoggedOnUser(DateOnlyPeriod period);
		IPersonRequest RetrieveRequest(Guid id);		
	}
}