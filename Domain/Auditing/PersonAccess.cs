using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Auditing
{
	public class PersonAccess : Entity, IPersonAccess, IEntity
	{
		public PersonAccess(Guid actionBy, Guid actionOn, string action, string actionResult, string actionData, Guid? correlation = null)
		{
			ActionPerformedBy = actionBy;
			ActionPerformeOn = actionOn;
			Action = action;
			ActionResult = actionResult;
			Data = actionData;
			Correlation = correlation;
		}

		public virtual DateTime TimeStamp { get; set; }
		public virtual Guid ActionPerformedBy { get; set; }
		public virtual Guid ActionPerformeOn { get; set; }
		public virtual string Action { get; set; }
		public virtual string ActionResult { get; set; }
		public virtual string Data { get; set; }
		public virtual Guid? Correlation { get; set; }
	}
}
