﻿using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.Provider
{
	public interface IRequestsProvider
	{
		IEnumerable<IPersonRequest> RetrieveRequests(AllRequestsFormData input, IEnumerable<RequestType> requestTypes, out int totalCount);
	}
}