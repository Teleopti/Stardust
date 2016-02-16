using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Domain.Logon
{
	public class AsSystemAspect : IAspect
	{
		private readonly TenantFromArguments _tenant;
		private readonly AsSystem _asSystem;

		public AsSystemAspect(TenantFromArguments tenant, AsSystem asSystem)
		{
			_tenant = tenant;
			_asSystem = asSystem;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			var tenant = _tenant.Resolve(invocation.Arguments);
			var businessUnitId = invocation.Arguments.Cast<ILogOnContext>().First().LogOnBusinessUnitId;
			_asSystem.Logon(tenant, businessUnitId);
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
		}
	}
}