using System;
using System.Collections;
using System.Linq;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Aspects
{
	public class DataSourceFromAuthenticationKeyAspect : IAspect
	{
		private readonly IDataSourceScope _dataSource;
		private readonly IApplicationData _applicationData;
		private readonly IFindTenantNameByRtaKey _findTenantNameByRtaKey;

		[ThreadStatic]
		private static IDisposable _scope;

		public DataSourceFromAuthenticationKeyAspect(IDataSourceScope dataSource, IApplicationData applicationData, IFindTenantNameByRtaKey findTenantNameByRtaKey)
		{
			_dataSource = dataSource;
			_applicationData = applicationData;
			_findTenantNameByRtaKey = findTenantNameByRtaKey;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			var argument = invocation.Arguments.First();

			var enumerable = argument as IEnumerable;
			dynamic input = enumerable != null ? enumerable.Cast<dynamic>().First() : argument;

			var key = input.AuthenticationKey;
			var tenant = _findTenantNameByRtaKey.Find(key);
			var dataSource = _applicationData.Tenant(tenant);
			_scope = _dataSource.OnThisThreadUse(dataSource);
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			_scope.Dispose();
		}
	}
}