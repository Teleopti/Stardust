using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.Provider
{
	public interface IRequestCommandHandlingProvider
	{
		RequestCommandHandlingResult ApproveRequests(IEnumerable<Guid> ids);
		RequestCommandHandlingResult DenyRequests(IEnumerable<Guid> ids);
		RequestCommandHandlingResult CancelRequests(IEnumerable<Guid> ids);
	}
}