using System;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Logon
{
	public class FullPermissionsAspect : IAspect
	{
		private readonly FullPermissions _fullPermissions;
		[ThreadStatic]
		private static IDisposable _scope;

		public FullPermissionsAspect(FullPermissions fullPermissions)
		{
			_fullPermissions = fullPermissions;
		}
		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			_scope = _fullPermissions.Apply();
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			_scope.Dispose();
			_scope = null;
		}
	}
}