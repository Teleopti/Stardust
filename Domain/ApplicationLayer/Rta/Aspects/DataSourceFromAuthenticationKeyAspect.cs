using System;
using System.Collections;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;
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

			string tenant;
			var key = tryGet(() => input.AuthenticationKey);
			if (key != null)
				tenant = _databaseLoader.TenantNameByKey(AuthenticationKeyEncodingFixer.Fix(key));
			else
				tenant = tryGet(() => input.Tenant);
			var dataSource = _dataSourceForTenant.Tenant(tenant);
			_scope = _dataSource.OnThisThreadUse(dataSource);
		}

		private static string tryGet(Func<string> input)
		{
			try
			{
				return input();
			}
			catch (RuntimeBinderException) { }
			return null;
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			_scope.Dispose();
		}
	}
}