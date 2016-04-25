using System;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Text.RegularExpressions;

namespace Manager.Integration.Test.WPF.Interceptors
{
	public class NoLockInterceptor : DbCommandInterceptor
	{
		private static readonly Regex TableAliasRegex =
			new Regex(@"(?<tableAlias>AS \[Extent\d+\](?! WITH \(NOLOCK\)))",
			          RegexOptions.Multiline | RegexOptions.IgnoreCase);

		[ThreadStatic]
		public static bool SuppressNoLock;

		public override void ScalarExecuting(DbCommand command,
		                                     DbCommandInterceptionContext<object> interceptionContext)
		{
			if (!SuppressNoLock)
			{
				command.CommandText =
					TableAliasRegex.Replace(command.CommandText, "${tableAlias} WITH (NOLOCK)");
			}
		}

		public override void ReaderExecuting(DbCommand command, 
											 DbCommandInterceptionContext<DbDataReader> interceptionContext)
		{
			if (!SuppressNoLock)
			{
				command.CommandText =
					TableAliasRegex.Replace(command.CommandText, "${tableAlias} WITH (NOLOCK)");
			}
		}
	}
}