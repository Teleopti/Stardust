using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.Provider
{
	public interface IRequestCommandHandlingProvider
	{
		IEnumerable<Guid> ApproveRequests(IEnumerable<Guid> ids);
		IEnumerable<Guid> DenyRequests(IEnumerable<Guid> ids);
	}
}