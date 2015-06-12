using System;
using System.IO;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Config
{
	public class ReadDataSourceConfigurationFromNhibFilesTest
	{
		[Test]
		public void ShouldParseFile()
		{
			const string nhibFile=@"<?xml version='1.0' encoding='utf-8'?>
<datasource>
	<hibernate-configuration xmlns='urn:nhibernate-configuration-2.2'>
		<session-factory name='sessionFactoryName'>
			<!-- properties -->
			<property name='connection.connection_string'>
				App connection string
			</property>
			<property name='command_timeout'>60</property>
		</session-factory>
	</hibernate-configuration>
	<matrix>
		<connectionString>
			Analytics connection string
		</connectionString>
	</matrix>
</datasource>
";
			var path = Path.GetTempPath();
			var filename = Path.Combine(path, Guid.NewGuid() + ".nhib.xml");

			try
			{
				File.WriteAllText(filename, nhibFile);
				var target = new ReadDataSourceConfigurationFromNhibFiles(new NhibFilePathFixed(path), new ParseNhibFile());
				var res = target.Read()["sessionFactoryName"];

				res.AnalyticsConnectionString.Should().Be.EqualTo("Analytics connection string");
				res.ApplicationConnectionString.Should().Be.EqualTo("App connection string");
				res.ApplicationNHibernateConfig["command_timeout"].Should().Be.EqualTo("60");
				res.ApplicationNHibernateConfig["connection.connection_string"].Should().Be.EqualTo("App connection string");
			}
			finally
			{
				File.Delete(filename);
			}
		}
	}
}