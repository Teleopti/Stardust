using System;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Web.Areas.People.Models;

namespace Teleopti.Ccc.Web.Areas.People.Core.Aspects
{
	public class AuditPersonAspect : IAspect
	{
		private readonly IAuditHelper auditHelper;
		public AuditPersonAspect(IAuditHelper auditHelper)
		{
			this.auditHelper = auditHelper;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			foreach (var arg in invocation.Arguments)
			{
				if (arg is GrantRolesInputModel)
				{
					auditHelper.AuditCall(PersonAuditActionType.GrantRole, arg as GrantRolesInputModel);

				}
				else if (arg is RevokeRolesInputModel)
				{
					auditHelper.AuditCall(PersonAuditActionType.RevokeRole, arg as RevokeRolesInputModel);
				}
			}
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
		}
	}
}
