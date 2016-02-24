using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Domain.Logon.Aspects
{
	public class TenantFromArguments
	{
		private readonly TenantLoader _tenants;

		public TenantFromArguments(TenantLoader tenants)
		{
			_tenants = tenants;
		}

		public string Resolve(IEnumerable<object> arguments)
		{
			var argument = arguments.First();
			if (argument is IEnumerable && !(argument is string))
				argument = (argument as IEnumerable).Cast<dynamic>().First();

			if (argument is string)
				return argument as string;

			if (argument is ILogOnContext)
				return (argument as ILogOnContext).LogOnDatasource;

			dynamic dynamic = argument;
			var key = tryGet(() => dynamic.AuthenticationKey);
			if (key != null)
				return _tenants.TenantNameByKey(LegacyAuthenticationKey.MakeEncodingSafe(key));
			return tryGet(() => dynamic.Tenant);

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

	}
}