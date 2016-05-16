using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Domain.Logon
{

	public class ImpersonateSystemAspect : IAspect
	{
		private readonly TenantFromArguments _tenant;
		private readonly ImpersonateSystem _impersonateSystem;

		public ImpersonateSystemAspect(TenantFromArguments tenant, ImpersonateSystem impersonateSystem)
		{
			_tenant = tenant;
			_impersonateSystem = impersonateSystem;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			var tenant = _tenant.Resolve(invocation.Arguments);
			var businessUnitId = invocation.Arguments.Cast<ILogOnContext>().First().LogOnBusinessUnitId;
			_impersonateSystem.Impersonate(tenant, businessUnitId);
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			_impersonateSystem.EndImpersonation();
		}
	}
}