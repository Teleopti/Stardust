using Teleopti.Ccc.Web.Areas.People.Models;

namespace Teleopti.Ccc.Web.Areas.People.Core.Aspects
{
	public class PersonAccessAuditContext : IHandleContext<GrantRolesInputModel>, IHandleContext<RevokeRolesInputModel>
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
}