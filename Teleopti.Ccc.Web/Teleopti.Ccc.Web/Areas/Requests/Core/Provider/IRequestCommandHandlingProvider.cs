using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.Provider
{
	public interface IRequestCommandHandlingProvider
	{
		RequestCommandHandlingResult ApproveRequests(IEnumerable<Guid> requestIds);
		RequestCommandHandlingResult ApproveWithValidators(IEnumerable<Guid> requestIds, RequestValidatorsFlag validators);
		RequestCommandHandlingResult DenyRequests(IEnumerable<Guid> requestIds);
		RequestCommandHandlingResult CancelRequests(IEnumerable<Guid> requestIds);
		RequestCommandHandlingResult RunWaitlist(DateTimePeriod period);
	}
}