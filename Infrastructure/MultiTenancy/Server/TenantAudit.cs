using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class TenantAudit
	{

		protected TenantAudit()
		{
		}

		public TenantAudit(Guid actionBy, Guid actionOn, string action, string actionResult, string actionData, Guid? correlation = null)
			: this()
		{
			this.Id = Guid.NewGuid();
			ActionPerformedBy = actionBy;
			ActionPerformedOn = actionOn;
			Action = action;
			ActionResult = actionResult;
			Data = actionData;
			Correlation = correlation ?? Guid.NewGuid();
			TimeStamp = DateTime.UtcNow;
		}

		public virtual Guid? Id { get; set; }
		public virtual DateTime TimeStamp { get; set; }
		public virtual Guid ActionPerformedBy { get; set; }
		public virtual Guid ActionPerformedOn { get; set; }
		public virtual string Action { get; set; }
		public virtual string ActionResult { get; set; }
		public virtual string Data { get; set; }
		public virtual Guid Correlation { get; set; }
	}
}
