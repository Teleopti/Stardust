using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.Requests.Core.Provider
{
	public interface IRequestCommandHandlingProvider
	{
		RequestCommandHandlingResult ApproveRequests(IEnumerable<Guid> requestIds, string replyMessage);

		RequestCommandHandlingResult ApproveWithValidators(IEnumerable<Guid> requestIds, RequestValidatorsFlag validators);

		RequestCommandHandlingResult DenyRequests(IEnumerable<Guid> requestIds, string replyMessage);

		RequestCommandHandlingResult CancelRequests(IEnumerable<Guid> requestIds, string replyMessage);

		RequestCommandHandlingResult ReplyRequests(IEnumerable<Guid> requestIds, string message);

		RequestCommandHandlingResult RunWaitlist(DateTimePeriod period);
	}
}