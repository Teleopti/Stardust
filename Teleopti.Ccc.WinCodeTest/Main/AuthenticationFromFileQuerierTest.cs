using System;
using System.IO;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;

namespace Teleopti.Ccc.WinCodeTest.Main
{
	[TestFixture]
	public class AuthenticationFromFileQuerierTest
	{
		private const string json = @"{""Success"":true,""FailReason"":null,""PersonId"":""10957ad5-5489-48e0-959a-9b5e015b2b5c"",""Tennant"":""Teleopti WFM"",""DataSource"":""<datasource>
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
		</datasource>""}";

		[Test]
		public void ShouldReadJsonFromFile()
		{
			var path = Path.GetTempPath();
			var filename = Guid.NewGuid() + ".json";
			var fullPath = path + filename;
			var writer = new StreamWriter(fullPath);

			writer.Write(json);
			writer.Close();

			var target = new AuthenticationFromFileQuerier(fullPath);

			target.TryIdentityLogon(new IdentityLogonClientModel(), null).Success.Should().Be.True();
			File.Delete(fullPath);
		}
	}
}