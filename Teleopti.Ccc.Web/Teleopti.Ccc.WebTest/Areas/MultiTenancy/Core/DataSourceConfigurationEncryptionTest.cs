using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core
{
	[TestFixture]
	public class DataSourceConfigurationEncryptionTest
	{
		[Test]
		public void ShouldEncryptAndDecryptValues()
		{
			var target = new DataSourceConfigurationEncryption();
			

			var dataSourceConfig = new DataSourceConfiguration(RandomName.Make(), "a very secret connectionstring");
			var encryptedThree = Encryption.EncryptStringToBase64("a very secret connectionstring", EncryptionConstants.Image1, EncryptionConstants.Image2);
			var enctyptedAppConnstring = Encryption.EncryptStringToBase64(dataSourceConfig.ApplicationConnectionString, EncryptionConstants.Image1, EncryptionConstants.Image2);

			dataSourceConfig = target.EncryptConfig(dataSourceConfig);

			dataSourceConfig.AnalyticsConnectionString.Should().Be.EqualTo(encryptedThree);
			dataSourceConfig.ApplicationConnectionString.Should().Be.EqualTo(enctyptedAppConnstring);
		}
	}
}