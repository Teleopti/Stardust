namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public interface INHibernateConfigurationsHandler
	{
		string GetConfigForName(string dataSourceName);
	}
}