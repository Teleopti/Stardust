using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface IPersonRequestProvider
	{
		IEnumerable<IPersonRequest> RetrieveTextRequests(Paging paging);
		IEnumerable<IPersonRequest> RetrieveRequests(DateOnlyPeriod period);
		IPersonRequest RetrieveRequest(Guid id);
	}
}