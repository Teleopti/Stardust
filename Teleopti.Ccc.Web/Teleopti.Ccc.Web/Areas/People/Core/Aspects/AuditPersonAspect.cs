using System;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Infrastructure.Audit;
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
				if (arg is GrantRolesInputModel model)
				{
					auditHelper.AuditCall(PersonAuditActionType.GrantRole, model);

				}
				else if (arg is RevokeRolesInputModel inputModel)
				{
					auditHelper.AuditCall(PersonAuditActionType.RevokeRole, inputModel);
				}
			}
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
		}
	}
}
