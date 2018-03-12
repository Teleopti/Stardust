using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Auditing
{
	public class PersonAccess : SimpleAggregateRoot, IPersonAccess
	{
		protected PersonAccess()
		{
		}

		public PersonAccess(IPerson actionBy, IPerson actionOn, string action, string actionResult, string actionData, Guid? correlation = null)
			: this()
		{
			ActionPerformedBy = actionBy;
			ActionPerformedOn = actionOn;
			Action = action;
			ActionResult = actionResult;
			Data = actionData;
			Correlation = correlation ?? Guid.NewGuid();
			TimeStamp = DateTime.UtcNow;
		}

		public virtual DateTime TimeStamp { get; set; }
		public virtual IPerson ActionPerformedBy { get; set; }
		public virtual IPerson ActionPerformedOn { get; set; }
		public virtual string Action { get; set; }
		public virtual string ActionResult { get; set; }
		public virtual string Data { get; set; }
		public virtual Guid Correlation { get; set; }
	}
}
