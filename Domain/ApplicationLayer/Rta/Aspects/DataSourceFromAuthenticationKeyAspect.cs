using System;
using System.Collections;
using System.Linq;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Aspects
{
	public class DataSourceFromAuthenticationKeyAspect : IDataSourceFromAuthenticationKeyAspect
	{
		private readonly IDataSourceScope _dataSource;
		private readonly IDataSourceForTenant _dataSourceForTenant;
		private readonly IDatabaseLoader _databaseLoader;

		[ThreadStatic]
		private static IDisposable _scope;

		public DataSourceFromAuthenticationKeyAspect(IDataSourceScope dataSource, IDataSourceForTenant dataSourceForTenant, IDatabaseLoader databaseLoader)
		{
			_dataSource = dataSource;
			_dataSourceForTenant = dataSourceForTenant;
			_databaseLoader = databaseLoader;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			var argument = invocation.Arguments.First();

			var enumerable = argument as IEnumerable;
			dynamic input = enumerable != null ? enumerable.Cast<dynamic>().First() : argument;

			var key = input.AuthenticationKey;
			key = AuthenticationKeyEncodingFixer.Fix(key);
			var tenant = _databaseLoader.TenantNameByKey(key);
			var dataSource = _dataSourceForTenant.Tenant(tenant);
			_scope = _dataSource.OnThisThreadUse(dataSource);
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			_scope.Dispose();
		}
	}
}