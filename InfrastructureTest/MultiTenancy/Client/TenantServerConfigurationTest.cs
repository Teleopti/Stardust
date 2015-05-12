using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	public class TenantServerConfigurationTest
	{
		[Test]
		public void ShouldGetPathIfEndingWithBackslash()
		{
			const string path = @"http://www.roger.com/";
			var relativePath = RandomName.Make();
			var target = new TenantServerConfiguration(path);
			target.FullPath(relativePath)
				.Should().Be.EqualTo(@"http://www.roger.com/" + relativePath);
		}

		[Test]
		public void ShouldGetPathIfNotEndingWithBackslash()
		{
			const string path = @"http://www.roger.com";
			var relativePath = RandomName.Make();
			var target = new TenantServerConfiguration(path);
			target.FullPath(relativePath)
				.Should().Be.EqualTo(@"http://www.roger.com/" + relativePath);
		}

		[Test]
		public void IfRelativePathIsEmptyReturnAsIsToGetFakeImplementationWorkCorrectly()
		{
			var path = RandomName.Make();
			var target = new TenantServerConfiguration(path);
			target.FullPath(string.Empty)
				.Should().Be.EqualTo(path);
		}

		[Test]
		public void ShouldReturnRelativePathIfPathIsNull()
		{
			var relativePath = RandomName.Make();
			var target = new TenantServerConfiguration(null);
			target.FullPath(relativePath)
				.Should().Be.EqualTo(relativePath);
		}
	}
}