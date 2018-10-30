using System;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	//TODO Toggle file?
	public class TenantAudit : SimpleAggregateRoot, ITenantAudit
	{

		protected TenantAudit()
		{
		}

		public TenantAudit(Guid actionBy, Guid actionOn, string action, string actionData, Guid? correlation = null)
			: this()
		{
			ActionPerformedBy = actionBy;
			ActionPerformedOn = actionOn;
			Action = action;
			ActionResult = string.Empty;
			Data = actionData;
			Correlation = correlation ?? Guid.NewGuid();
			TimeStamp = DateTime.UtcNow;
		}

		public virtual DateTime TimeStamp { get; set; }
		public virtual Guid ActionPerformedBy { get; set; }
		public virtual Guid ActionPerformedOn { get; set; }
		public virtual string Action { get; set; }
		public virtual string ActionResult { get; set; }
		public virtual string Data { get; set; }
		public virtual Guid Correlation { get; set; }
	}
}
