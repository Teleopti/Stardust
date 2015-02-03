using System;
using System.IO;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Tenant.Core;
using Teleopti.Ccc.Web.Core.Startup.InitializeApplication;

namespace Teleopti.Ccc.WebTest.Areas.Tenant.Core
{
	public class NHibernateConfigurationsHandlerTest
	{
		private const string nhibConfContent = @"
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
		</datasource>
";

		private const string nhibConf = @"<?xml version='1.0' encoding='utf-8'?>" + nhibConfContent;

	
		[Test]
		public void ShouldLoadDataSources()
		{
			var settings = MockRepository.GenerateMock<ISettings>();
			var path = Path.GetTempPath();
			var filename = Guid.NewGuid() + ".nhib.xml";
			var writer = new StreamWriter(path + filename);

			writer.Write(nhibConf);
			writer.Close();
			settings.Stub(x => x.nhibConfPath()).Return(path);

			var target = new NHibernateConfigurationsHandler(settings, new physicalApplicationPathStub(path));
			var result = target.GetConfigForName("Teleopti WFM");
			result.Should().Not.Be.EqualTo(nhibConfContent);
			File.Delete(path + filename);
		}

		private class physicalApplicationPathStub : IPhysicalApplicationPath
		{
			private readonly string _path;

			public physicalApplicationPathStub(string path)
			{
				_path = path;
			}

			public string Get()
			{
				return _path;
			}
		}
	}
}