﻿using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security
{
	[TestFixture]
	public class DataSourceConfigDecryptionTest
	{
		[Test]
		public void ShouldEncryptAndDecryptValues()
		{
			var target = new DataSourceConfigDecryption();

			var dic = new Dictionary<string, string>
			{
				{"key1", "some secret value"},
				{"key2", "another secret value"}
			};

			var dataSourceConfig = new DataSourceConfig
			{
				ApplicationNHibernateConfig = dic,
				AnalyticsConnectionString = "a very secret connectionstring",
				ApplicationConnectionString = RandomName.Make()
			};

			var encryptedOne = Encryption.EncryptStringToBase64(dic["key1"], EncryptionConstants.Image1, EncryptionConstants.Image2);
			var encryptedTwo = Encryption.EncryptStringToBase64(dic["key2"], EncryptionConstants.Image1, EncryptionConstants.Image2);
			var encryptedThree = Encryption.EncryptStringToBase64("a very secret connectionstring", EncryptionConstants.Image1, EncryptionConstants.Image2);
			var encryptedAppConnectionString = Encryption.EncryptStringToBase64(dataSourceConfig.ApplicationConnectionString, EncryptionConstants.Image1, EncryptionConstants.Image2);

			var dataSourceConfigEncrypted = target.EncryptConfigJustForTest(dataSourceConfig);

			dataSourceConfigEncrypted.AnalyticsConnectionString.Should().Be.EqualTo(encryptedThree);
			dataSourceConfigEncrypted.ApplicationNHibernateConfig["key1"].Should().Be.EqualTo(encryptedOne);
			dataSourceConfigEncrypted.ApplicationNHibernateConfig["key2"].Should().Be.EqualTo(encryptedTwo);
			dataSourceConfigEncrypted.ApplicationConnectionString.Should().Be.EqualTo(encryptedAppConnectionString);

			var result = target.DecryptConfig(dataSourceConfigEncrypted);

			result.AnalyticsConnectionString.Should().Be.EqualTo("a very secret connectionstring");
			result.ApplicationNHibernateConfig["key1"].Should().Be.EqualTo("some secret value");
			result.ApplicationNHibernateConfig["key2"].Should().Be.EqualTo("another secret value");
			result.ApplicationConnectionString.Should().Be.EqualTo(dataSourceConfig.ApplicationConnectionString);
		}
	}
}