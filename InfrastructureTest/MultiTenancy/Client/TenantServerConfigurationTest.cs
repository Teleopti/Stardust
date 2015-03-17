using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	public class TenantServerConfigurationTest
	{
		[Test]
		public void ShouldGetPath()
		{
			var path = RandomName.Make();
			var target = new TenantServerConfiguration(path);
			target.Path
				.Should().Be.EqualTo(path);
		}
	}
}