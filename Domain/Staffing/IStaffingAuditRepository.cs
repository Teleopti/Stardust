using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public interface IStaffingAuditRepository
	{
		void Persist(StaffingAudit staffingAudit);
	}

	public class StaffingAudit
	{
		protected StaffingAudit()
		{
		}

		public StaffingAudit(IPerson actionBy, IPerson actionOn, string action, string actionResult, string actionData, Guid? correlation = null)
			: this()
		{
			//do we need action performed on?
			ActionPerformedBy = actionBy;
			ActionPerformedOn = actionOn;
			Action = action;
			ActionResult = actionResult;
			Data = actionData;
			Correlation = correlation ?? Guid.NewGuid();
			TimeStamp = DateTime.UtcNow;
		}

		public DateTime TimeStamp { get; set; }
		public IPerson ActionPerformedBy { get; set; }
		public IPerson ActionPerformedOn { get; set; }
		public string Action { get; set; }
		public string ActionResult { get; set; }
		public string Data { get; set; }
		public Guid Correlation { get; set; }
	}
}