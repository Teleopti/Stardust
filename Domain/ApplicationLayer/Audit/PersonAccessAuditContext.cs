using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Audit
{
	public class PersonAccessAuditContext : IHandleContextAction<GrantRolesInputModel>, IHandleContextAction<RevokeRolesInputModel>
	{
		private readonly IAuditHelper auditHelper;

		public PersonAccessAuditContext(IAuditHelper auditHelper)
		{
			this.auditHelper = auditHelper;
		}

		public void Handle(GrantRolesInputModel command)
		{
			auditHelper.AuditCall(PersonAuditActionType.GrantRole, command);
		}


		public void Handle(RevokeRolesInputModel command)
		{
			auditHelper.AuditCall(PersonAuditActionType.RevokeRole, command);
		}
	}

	public class RevokeRolesInputModel : PersonRolesBaseModel
	{
	}

	public class GrantRolesInputModel : PersonRolesBaseModel
	{
	}

	public class PersonRolesBaseModel
	{
		public IEnumerable<Guid> Persons { get; set; }
		public IEnumerable<Guid> Roles { get; set; }
	}

	public interface IAuditHelper
	{
		void AuditCall(PersonAuditActionType actionType, PersonRolesBaseModel inputmodel);
	}
}