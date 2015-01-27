using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.WinCodeTest.Main
{
	[TestFixture]
	public class ApplicationDataDataSourceTest
	{

		[Test]
		public void ShouldAddDataSourceIfNotExists()
		{
			const string nhibConf = @"<?xml version='1.0' encoding='utf-8'?>
		<datasource>
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

			const string nhibConf2 = @"<?xml version='1.0' encoding='utf-8'?>
		<datasource>
			<hibernate-configuration xmlns='urn:nhibernate-configuration-2.2'>
				<session-factory name='Teleopti WFM2'>
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
			var appSettings = new Dictionary<string, string>();
			var dataSourcesFactory = MockRepository.GenerateMock<IDataSourcesFactory>();
			var target = new ApplicationData(appSettings, MockRepository.GenerateMock<IMessageBrokerComposite>(),
				MockRepository.GenerateMock<ILoadPasswordPolicyService>(), dataSourcesFactory);
			var dataSource1 = MockRepository.GenerateMock<IDataSource>();
			var dataSource2 = MockRepository.GenerateMock<IDataSource>();
			
			dataSourcesFactory.Stub(x => x.TryCreate(XElement.Parse(nhibConf), out dataSource1)).OutRef(dataSource1).Return(true).IgnoreArguments();
			dataSourcesFactory.Stub(x => x.TryCreate(XElement.Parse(nhibConf2), out dataSource1)).OutRef(dataSource2).Return(true).IgnoreArguments();
			dataSource1.Stub(x => x.DataSourceName).Return("Teleopti WFM");
			dataSource2.Stub(x => x.DataSourceName).Return("Teleopti WFM2");
			target.RegisteredDataSourceCollection.Should().Be.Empty();
			target.CreateAndAddDataSource(nhibConf);
			target.RegisteredDataSourceCollection.Count().Should().Be.EqualTo(1);
			target.CreateAndAddDataSource(nhibConf);
			target.RegisteredDataSourceCollection.Count().Should().Be.EqualTo(1);
			target.CreateAndAddDataSource(nhibConf2);
			target.RegisteredDataSourceCollection.Count().Should().Be.EqualTo(2);
		}
	}
}