using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	public interface INHibernateConfigurationsHandler
	{
		string GetConfigForName(string dataSourceName);
	}

	public class NHibernateConfigurationsHandler
	{
		readonly IList<IDataSourceHolder> _dataSources = new List<IDataSourceHolder>();
		private string nhibConf = @"<datasource>
	<hibernate-configuration xmlns='urn:nhibernate-configuration-2.2'>
		<session-factory name='Teleopti WFM'>
			<property name='connection.connection_string'>
		
        Data Source=.;Integrated Security=SSPI;Initial Catalog=main_clone_DemoSales_TeleoptiCCC7;Current Language=us_english
      </property>
			<property name='command_timeout'>60</property>
		</session-factory>
	</hibernate-configuration>
	<matrix name='MatrixDatamartDemo'>
		<connectionString>
		<!--WISEMETA: default='[SQL_AUTH_STRING];Initial Catalog=[DB_ANALYTICS];Current Language=us_english'-->
		Data Source=.;Integrated Security=SSPI;Initial Catalog=main_clone_DemoSales_TeleoptiAnalytics;Current Language=us_english
    </connectionString>
	</matrix>
	<authentication>
		<logonMode>mix</logonMode>
		<!--  win or mix -->
	</authentication>
</datasource>";

		public NHibernateConfigurationsHandler()
		{
			//behöver veta nhibkatalogen sedan

			_dataSources.Add(new DataSourceHolder{DataSourceName = "Teleopti WFM", DataSourceConfig = Encryption.EncryptStringToBase64(nhibConf,
																		  EncryptionConstants.Image1,
																		  EncryptionConstants.Image2)});
			
		}
		public string GetConfigForName(string dataSourceName)
		{
			foreach (var dataSourceHolder in _dataSources.Where(dataSourceHolder => dataSourceHolder.DataSourceName.Equals(dataSourceName)))
			{
				return dataSourceHolder.DataSourceConfig;
			}
			return "";
		}
	}

	interface IDataSourceHolder
	{
		string DataSourceName { get; set; }
		string DataSourceConfig { get; set; }
	}

	class DataSourceHolder : IDataSourceHolder
	{
		public string DataSourceName { get; set; }
		public string DataSourceConfig { get; set; }
	}
}