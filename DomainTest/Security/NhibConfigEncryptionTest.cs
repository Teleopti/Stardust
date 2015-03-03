using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security
{
	[TestFixture]
	public class NhibConfigEncryptionTest
	{
		private readonly NhibConfigEncryption _target = new NhibConfigEncryption();

		[Test]
		public void ShouldEncryptAndDecryptValues()
		{
			var dic = new Dictionary<string, string>
			{
				{"key1", "some secret value"},
				{"key2", "another secret value"},
				{"key3", "a third secret value"}
			};

			var dataSourceConfig = new DataSourceConfig
			{
				ApplicationNHibernateConfig = dic,
				AnalyticsConnectionString = "a very secret connectionstring"
			};

			var encryptedOne = Encryption.EncryptStringToBase64(dic["key1"], EncryptionConstants.Image1, EncryptionConstants.Image2);
			var encryptedTwo = Encryption.EncryptStringToBase64(dic["key2"], EncryptionConstants.Image1, EncryptionConstants.Image2);
			var encryptedThree = Encryption.EncryptStringToBase64("a very secret connectionstring", EncryptionConstants.Image1, EncryptionConstants.Image2);

			dataSourceConfig = _target.EncryptConfig(dataSourceConfig);

			dataSourceConfig.AnalyticsConnectionString.Should().Be.EqualTo(encryptedThree);
			dataSourceConfig.ApplicationNHibernateConfig["key1"].Should().Be.EqualTo(encryptedOne);
			dataSourceConfig.ApplicationNHibernateConfig["key2"].Should().Be.EqualTo(encryptedTwo);

			dataSourceConfig = _target.DecryptConfig(dataSourceConfig);

			dataSourceConfig.AnalyticsConnectionString.Should().Be.EqualTo("a very secret connectionstring");
			dataSourceConfig.ApplicationNHibernateConfig["key1"].Should().Be.EqualTo("some secret value");
			dataSourceConfig.ApplicationNHibernateConfig["key2"].Should().Be.EqualTo("another secret value");
		}

	}
}