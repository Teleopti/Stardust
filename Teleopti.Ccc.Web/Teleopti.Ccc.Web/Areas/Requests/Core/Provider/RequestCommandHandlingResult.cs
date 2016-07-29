using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.Provider
{
	public class RequestCommandHandlingResult
	{
		public RequestCommandHandlingResult(ICollection<Guid> affectedRequestIds, ICollection<string> errorMessages)
		{
			var success = false;
			
			if (errorMessages != null)
			{
				success = errorMessages.Count == 0;
				ErrorMessages = errorMessages.ToArray();
			}
			if (affectedRequestIds != null)
			{
				AffectedRequestIds = affectedRequestIds.ToArray();
				success = affectedRequestIds.Count > 0;
			}

			Success = success;
		}

		public RequestCommandHandlingResult(ICollection<Guid> affectedRequestIds, ICollection<string> errorMessages,
			Guid commandTrackId)
			: this(affectedRequestIds, errorMessages)
		{
			CommandTrackId = commandTrackId;
		}

		public RequestCommandHandlingResult(ICollection<Guid> affectedRequestIds, ICollection<string> errorMessages,
			int replySuccessCount)
			: this(affectedRequestIds, errorMessages)
		{
			ReplySuccessCount = replySuccessCount;
		}

		public bool Success { get; set; }

		public int ReplySuccessCount { get; set; }

		public IEnumerable<Guid> AffectedRequestIds { get; set; }

		public IEnumerable<string> ErrorMessages { get; set; }

		//used only in MyTimeWeb
		public RequestViewModel RequestViewModel { get; set; }

		public Guid CommandTrackId { get; set; }
	}
}