using System;
using System.IO;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Tenant.Core;
using Teleopti.Ccc.Web.Core.Startup.InitializeApplication;

namespace Teleopti.Ccc.WebTest.Areas.Tenant.Core
{
	public class ReadNHibFilesTest
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
				var settings = MockRepository.GenerateMock<ISettings>();
				settings.Stub(x => x.nhibConfPath()).Return(path);

				File.WriteAllText(filename, nhibFile);
				var target = new ReadNHibFiles(settings, new physicalApplicationPathStub(path), new ParseNhibFile());
				var res = target.Read()["sessionFactoryName"];

				res.AnalyticsConnectionString.Should().Be.EqualTo("Analytics connection string");
				res.ApplicationNHibernateConfig["command_timeout"].Should().Be.EqualTo("60");
				res.ApplicationNHibernateConfig["connection.connection_string"].Should().Be.EqualTo("App connection string");
			}
			finally
			{
				File.Delete(filename);
			}
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