using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.Security
{
	[TestFixture]
	public class DataSourceConfigDecryptionTest
	{
		[Test]
		public void ShouldEncryptAndDecryptValues()
		{
			var target = new DataSourceConfigDecryption();
			
			var dataSourceConfig = new DataSourceConfig
			{
				AnalyticsConnectionString = "a very secret connectionstring",
				ApplicationConnectionString = RandomName.Make()
			};
			
			var encryptedThree = Encryption.EncryptStringToBase64("a very secret connectionstring", EncryptionConstants.Image1, EncryptionConstants.Image2);
			var encryptedAppConnectionString = Encryption.EncryptStringToBase64(dataSourceConfig.ApplicationConnectionString, EncryptionConstants.Image1, EncryptionConstants.Image2);

			var dataSourceConfigEncrypted = target.EncryptConfigJustForTest(dataSourceConfig);

			dataSourceConfigEncrypted.AnalyticsConnectionString.Should().Be.EqualTo(encryptedThree);
			dataSourceConfigEncrypted.ApplicationConnectionString.Should().Be.EqualTo(encryptedAppConnectionString);

			var result = target.DecryptConfig(dataSourceConfigEncrypted);

			result.AnalyticsConnectionString.Should().Be.EqualTo("a very secret connectionstring");
			result.ApplicationConnectionString.Should().Be.EqualTo(dataSourceConfig.ApplicationConnectionString);
		}
	}
}