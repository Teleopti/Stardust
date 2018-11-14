using System;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class StaffingAudit : SimpleAggregateRoot, IStaffingAudit

	{
		protected StaffingAudit()
		{
		}

		public StaffingAudit(IPerson actionBy, string action, string area, string importFileName, Guid? bpoId = null, DateTime? clearPeriodStart = null, DateTime? clearPeriodEnd=null)
			: this()
		{
			ActionPerformedById = actionBy.Id.GetValueOrDefault();
			ActionPerformedBy = createSerializedPersonAuditInfo(actionBy);
			Action = action;
			TimeStamp = DateTime.UtcNow;
			Area = area;
			ImportFileName = importFileName;
			BpoId = bpoId;
			ClearPeriodStart = clearPeriodStart;
			ClearPeriodEnd = clearPeriodEnd;
		}

		//public StaffingAudit(IPerson actionBy, string action, string area, string importFileName)
		//	: this()
		//{
		//	ActionPerformedBy = actionBy;
		//	Action = action;
		//	TimeStamp = DateTime.UtcNow;
		//	Area = area;
		//	ImportFileName = importFileName;
		//	BpoName = "";
		//	ClearPeriodStart = null;
		//	ClearPeriodEnd = null;
		//}

		public virtual DateTime TimeStamp { get; set; }
		public virtual Guid ActionPerformedById { get; set; }
		//public IPerson ActionPerformedOn { get; set; }
		public virtual string Action { get; set; }
		public virtual string Area { get; set; }
		public virtual string ActionPerformedBy { get; set; }
		public virtual string ImportFileName { get; set; }
		public virtual Guid? BpoId { get; set; }
		public virtual DateTime? ClearPeriodStart { get; set; }
		public virtual DateTime? ClearPeriodEnd { get; set; }

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
	}

	public class PersonAuditInfo : ILightPerson
	{
		public virtual string FirstName { get; set; }
		public virtual string LastName { get; set; }
		public virtual string EmploymentNumber { get; set; }
		public virtual string Email { get; set; }
	}
}
