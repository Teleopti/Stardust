using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;

namespace Teleopti.Ccc.Domain.Logon.Aspects
{
	public interface ITenantFinder
	{
		string Find(object argument);
	}

	public class TenantFromArguments
	{
		private readonly IEnumerable<ITenantFinder> _finders;

		public TenantFromArguments(IEnumerable<ITenantFinder> finders)
		{
			_finders = finders;
		}

		public string Resolve(IEnumerable<object> arguments)
		{
			var argument = arguments.First();
			if (argument is IEnumerable && !(argument is string))
				argument = (argument as IEnumerable).Cast<dynamic>().First();

			if (argument is string s)
				return s;

			if (argument is ILogOnContext context)
				return context.LogOnDatasource;

			// make all above n below ITenantFinder implementations?
			var tenant = _finders
				.Select(x => x.Find(argument))
				.FirstOrDefault(x => x != null);

			if (tenant != null)
				return tenant;
			
			dynamic dynamic = argument;
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