namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public static class DataSourceSettingValues
	{
		public static string SqlServerExceptionConverterTypeName { get { return typeof(SqlServerExceptionConverter).AssemblyQualifiedName; } }
	}
}