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

		public StaffingAudit(IPerson actionBy, string action, string actionData, string area)
			: this()
		{
			ActionPerformedBy = actionBy;
			Action = action;
			Data = actionData;
			TimeStamp = DateTime.UtcNow;
			Area = area;
		}

		public virtual DateTime TimeStamp { get; set; }

		public virtual IPerson ActionPerformedBy { get; set; }

		//public IPerson ActionPerformedOn { get; set; }
		public virtual string Action { get; set; }
		public virtual string Data { get; set; }
		public virtual string Area { get; set; }
	}
}