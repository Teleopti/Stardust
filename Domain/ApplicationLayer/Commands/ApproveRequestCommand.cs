using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class ApproveRequestCommand : IRequestCommand
	{
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public Guid PersonRequestId { get; set; }
		public bool IsAutoGrant { get; set; } = false;
		public bool IgnoreErrorMessageForApprovedRequest { get; set; } = true;
		public Guid? AffectedRequestId { get; set; }
		public IList<string> ErrorMessages { get; set; }
		public string ReplyMessage { get; set; }
		public bool IsReplySuccess { get; set; }
		public IDictionary<DateTimePeriod,IList<ISkill>> OvertimeValidatedSkillDictionary { get; set; }
	}
}
