using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.Logon.Aspects
{
	public class TenantScopeAspect : IAspect
	{
		private readonly IDataSourceScope _dataSource;
		private readonly TenantFromArguments _tenant;

		[ThreadStatic]
		private static Stack<IDisposable> _scope;

		public TenantScopeAspect(IDataSourceScope dataSource, TenantFromArguments tenant)
		{
			_dataSource = dataSource;
			_tenant = tenant;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			if (_scope == null)
				_scope = new Stack<IDisposable>();
			_scope.Push(_dataSource.OnThisThreadUse(_tenant.Resolve(invocation.Arguments)));
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			_scope.Pop().Dispose();
		}
	}
}