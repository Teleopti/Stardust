using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.Provider
{
	public interface IRequestCommandHandlingProvider
	{
		RequestCommandHandlingResult ApproveRequests(IEnumerable<Guid> requestIds);
		RequestCommandHandlingResult ApproveWithValidators(IEnumerable<Guid> requestIds);
		RequestCommandHandlingResult DenyRequests(IEnumerable<Guid> requestIds);
		RequestCommandHandlingResult CancelRequests(IEnumerable<Guid> requestIds);
		RequestCommandHandlingResult RunWaitlist(DateTimePeriod period);
	}
}