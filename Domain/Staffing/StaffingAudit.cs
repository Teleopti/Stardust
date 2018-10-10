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

		public StaffingAudit(IPerson actionBy, string action, string actionResult, string actionData,
			Guid? correlation = null)
			: this()
		{
			//do we need action performed on?
			ActionPerformedBy = actionBy;
			//ActionPerformedOn = actionOn;
			Action = action;
			ActionResult = actionResult;
			Data = actionData;
			//should we have that or not?
			Correlation = correlation ?? Guid.NewGuid();
			TimeStamp = DateTime.UtcNow;
		}

		public virtual DateTime TimeStamp { get; set; }

		public virtual IPerson ActionPerformedBy { get; set; }

		//public IPerson ActionPerformedOn { get; set; }
		public virtual string Action { get; set; }
		public virtual string ActionResult { get; set; }
		public virtual string Data { get; set; }
		public virtual Guid Correlation { get; set; }
	}

	public interface IStaffingAudit : IAggregateRoot
	{
		DateTime TimeStamp { get; set; }
		IPerson ActionPerformedBy { get; set; }
		string Action { get; set; }
		string ActionResult { get; set; }
		string Data { get; set; }
		Guid Correlation { get; set; }
	}
}