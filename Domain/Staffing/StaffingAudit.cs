using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class StaffingAudit : SimpleAggregateRoot, IStaffingAudit

	{
		protected StaffingAudit()
		{
		}

		public StaffingAudit(IPerson actionBy, string action, string actionData, string context)
			: this()
		{
			ActionPerformedBy = actionBy;
			Action = action;
			Data = actionData;
			TimeStamp = DateTime.UtcNow;
			Context = context;
		}

		public virtual DateTime TimeStamp { get; set; }

		public virtual IPerson ActionPerformedBy { get; set; }

		//public IPerson ActionPerformedOn { get; set; }
		public virtual string Action { get; set; }
		public virtual string Data { get; set; }
		public virtual string Context { get; set; }
	}

	public interface IStaffingAudit : IAggregateRoot
	{
		DateTime TimeStamp { get; set; }
		IPerson ActionPerformedBy { get; set; }
		string Action { get; set; }
		string Data { get; set; }
		string Context { get; set; }
	}
}