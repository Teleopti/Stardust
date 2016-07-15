using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	[Flags]
	public enum RequestValidatorsFlag
	{
		None = 0,
		WriteProtectedScheduleValidator = 1 << 0,
		BudgetAllotmentValidator = 1 << 1
	}

	public class ApproveBatchRequestsCommand : ITrackableCommand, IErrorAttachedCommand
	{
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public RequestValidatorsFlag Validator { get; set; }
		public IEnumerable<Guid> PersonRequestIdList { get; set; }
		public IList<string> ErrorMessages { get; set; }
	}
}