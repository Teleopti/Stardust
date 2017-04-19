using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core
{
	[TestFixture]
	public class DataSourceConfigurationEncryptionTest
	{
		[Test]
		public void ShouldEncryptAndDecryptValues()
		{
			var target = new DataSourceConfigurationEncryption();
			var dic = new Dictionary<string, string>
			{
				{"key1", "some secret value"},
				{"key2", "another secret value"},
				{"key3", "a third secret value"}
			};

			var dataSourceConfig = new DataSourceConfiguration(RandomName.Make(), "a very secret connectionstring", dic);
			var encryptedOne = Encryption.EncryptStringToBase64(dic["key1"], EncryptionConstants.Image1, EncryptionConstants.Image2);
			var encryptedTwo = Encryption.EncryptStringToBase64(dic["key2"], EncryptionConstants.Image1, EncryptionConstants.Image2);
			var encryptedThree = Encryption.EncryptStringToBase64("a very secret connectionstring", EncryptionConstants.Image1, EncryptionConstants.Image2);
			var enctyptedAppConnstring = Encryption.EncryptStringToBase64(dataSourceConfig.ApplicationConnectionString, EncryptionConstants.Image1, EncryptionConstants.Image2);

			dataSourceConfig = target.EncryptConfig(dataSourceConfig);

			dataSourceConfig.AnalyticsConnectionString.Should().Be.EqualTo(encryptedThree);
			dataSourceConfig.ApplicationConfig["key1"].Should().Be.EqualTo(encryptedOne);
			dataSourceConfig.ApplicationConfig["key2"].Should().Be.EqualTo(encryptedTwo);
			dataSourceConfig.ApplicationConnectionString.Should().Be.EqualTo(enctyptedAppConnstring);
		}
	}
}