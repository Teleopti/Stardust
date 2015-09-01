using System;
using System.Collections;
using System.Linq;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Aspects
{
	public class DataSourceFromAuthenticationKeyAspect : IAspect
	{
		private readonly IDataSourceScope _dataSource;
		private readonly IApplicationData _applicationData;
		private readonly IDatabaseLoader _databaseLoader;

		[ThreadStatic]
		private static IDisposable _scope;

		public DataSourceFromAuthenticationKeyAspect(IDataSourceScope dataSource, IApplicationData applicationData, IDatabaseLoader databaseLoader)
		{
			_dataSource = dataSource;
			_applicationData = applicationData;
			_databaseLoader = databaseLoader;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			var argument = invocation.Arguments.First();

			var enumerable = argument as IEnumerable;
			dynamic input = enumerable != null ? enumerable.Cast<dynamic>().First() : argument;

			var key = input.AuthenticationKey;
			var tenant = _databaseLoader.TenantNameByKey(key);
			var dataSource = _applicationData.Tenant(tenant);
			_scope = _dataSource.OnThisThreadUse(dataSource);
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			_scope.Dispose();
		}
	}
}