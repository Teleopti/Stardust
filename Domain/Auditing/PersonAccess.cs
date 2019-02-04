using System;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Domain.Auditing
{
	public class PersonAccess : AggregateRoot, IPersonAccess
	{
		protected PersonAccess()
		{
		}

		public PersonAccess(IPerson actionBy, IPerson actionOn, string action, string actionResult, string actionData,string searchKeys, Guid? correlation = null)
			: this()
		{
			ActionPerformedById = actionBy.Id.GetValueOrDefault();
			ActionPerformedBy = createSerializedPersonAuditInfo(actionBy);
			ActionPerformedOnId = actionOn.Id.GetValueOrDefault();
			ActionPerformedOn = createSerializedPersonAuditInfo(actionOn);
			Action = action;
			ActionResult = actionResult;
			Data = actionData;
			Correlation = correlation ?? Guid.NewGuid();
			TimeStamp = DateTime.UtcNow;
			SearchKeys = searchKeys;
		}

		private string createSerializedPersonAuditInfo(IPerson person)
		{
			PersonAuditInfo personInfo = new PersonAuditInfo()
			{
				FirstName = person.Name.FirstName,
				LastName = person.Name.LastName,
				EmploymentNumber = person.EmploymentNumber,
				Email = person.Email
			};
			return JsonConvert.SerializeObject(personInfo);
		}

		public virtual DateTime TimeStamp { get; set; }
		public virtual Guid ActionPerformedById { get; set; }
		public virtual string ActionPerformedBy { get; set; }
		public virtual string ActionPerformedOn { get; set; }
		public virtual Guid ActionPerformedOnId { get; set; }
		public virtual string Action { get; set; }
		public virtual string ActionResult { get; set; }
		public virtual string Data { get; set; }
		public virtual Guid Correlation { get; set; }
		public virtual string SearchKeys { get; set; }
	}
}
