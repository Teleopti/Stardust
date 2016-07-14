using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	[Flags]
	public enum RequestValidatorsFlag
	{
		None,
		WriteProtectedScheduleValidator,
		BudgetAllotmentValidator
	}

	public class ApproveBatchRequestsCommand : ITrackableCommand, IErrorAttachedCommand
	{
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public RequestValidatorsFlag Validator { get; set; }
		public IEnumerable<Guid> PersonRequestIdList { get; set; }
		public IList<string> ErrorMessages { get; set; }
	}
}