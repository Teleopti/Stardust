using System;
using System.IO;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	public class SharedSettingsQuerierForNoWebTest
	{
		[Test]
		public void ShouldReadQueue()
		{
			const string json = @"{
	""Queue"":""arne""
}";
			var path = Path.GetTempPath();
			var filename = Guid.NewGuid() + ".json";
			var fullPath = path + filename;
			try
			{
				File.WriteAllText(fullPath, json);
				var target = new SharedSettingsQuerierForNoWeb(fullPath);
				var result = target.GetSharedSettings();
				result.Queue.Should().Be.EqualTo("arne");
			}
			finally
			{
				File.Delete(fullPath);
			}
		}
	}
}