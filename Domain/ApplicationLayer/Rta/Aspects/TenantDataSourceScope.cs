using System;
using System.Collections;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Aspects
{
	public class TenantDataSourceScope : IRtaDataSourceScope
	{
		private readonly IDataSourceScope _dataSource;
		private readonly ITenantLoader _tenantLoader;

		[ThreadStatic]
		private static IDisposable _scope;

		public TenantDataSourceScope(IDataSourceScope dataSource, ITenantLoader tenantLoader)
		{
			_dataSource = dataSource;
			_tenantLoader = tenantLoader;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			var argument = invocation.Arguments.First();
			var enumerable = argument as IEnumerable;
			dynamic input = enumerable != null ? enumerable.Cast<dynamic>().First() : argument;

			string tenant;
			var key = tryGet(() => input.AuthenticationKey);
			if (key != null)
				tenant = _tenantLoader.TenantNameByKey(AuthenticationKeyEncodingFixer.Fix(key));
			else
				tenant = tryGet(() => input.Tenant);
			_scope = _dataSource.OnThisThreadUse(tenant);
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