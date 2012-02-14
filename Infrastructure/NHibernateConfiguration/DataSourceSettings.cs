using NHibernate.Cfg;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public static class DataSourceSettings
	{
		public const string ConnectionProvider = Environment.ConnectionProvider;
		public const string Dialect = Environment.Dialect;
		public const string ConnectionString = Environment.ConnectionString;
		public const string SqlExceptionConverter = Environment.SqlExceptionConverter;
		public const string CurrentSessionContextClass = Environment.CurrentSessionContextClass;
		public const string CommandTimeout = Environment.CommandTimeout;
	}
}