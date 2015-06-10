﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core
{
	public class DataSourceConfigurationProviderUsingNhibFilesTest
	{
		[Test]
		public void ExistingConfiguration()
		{
			var expected = new DataSourceConfiguration();
			var readNHibFiles = MockRepository.GenerateMock<IReadDataSourceConfiguration>();
			var encrypter = MockRepository.GenerateMock<INhibConfigurationEncryption>();
			readNHibFiles.Expect(x => x.Read()).Return(new Dictionary<string, DataSourceConfiguration> {{"something", expected}});
			encrypter.Stub(x => x.EncryptConfig(expected)).Return(expected);
			var target = new DataSourceConfigurationProviderUsingNhibFiles(readNHibFiles, encrypter);
			target.ForTenant(new Tenant("something"))
				.Should().Be.SameInstanceAs(expected);
		}

		[Test]
		public void NonExistingConfiguration()
		{
			var readNHibFiles = MockRepository.GenerateMock<IReadDataSourceConfiguration>();
			var encrypter = MockRepository.GenerateMock<INhibConfigurationEncryption>();
			readNHibFiles.Expect(x => x.Read()).Return(new Dictionary<string, DataSourceConfiguration>());
			var target = new DataSourceConfigurationProviderUsingNhibFiles(readNHibFiles, encrypter);
			target.ForTenant(new Tenant("notsomething"))
				.Should().Be.Null();
		}

		[Test]
		public void ShouldKeepStateIfReadNhibFiles()
		{
			var readNHibFiles = MockRepository.GenerateMock<IReadDataSourceConfiguration>();
			var encrypter = MockRepository.GenerateMock<INhibConfigurationEncryption>();
			readNHibFiles.Expect(x => x.Read()).Return(new Dictionary<string, DataSourceConfiguration>());

			var target = new DataSourceConfigurationProviderUsingNhibFiles(readNHibFiles,encrypter);
			target.ForTenant(new Tenant("1"));
			target.ForTenant(new Tenant("1"));
			target.ForTenant(new Tenant("2"));

			readNHibFiles.AssertWasCalled(x => x.Read(), x => x.Repeat.Once());
		}

		[Test]
		public void ExistingMultipleConfigurations()
		{
			var expected = new DataSourceConfiguration();
			var expectedTwo = new DataSourceConfiguration();
			var readNHibFiles = MockRepository.GenerateMock<IReadDataSourceConfiguration>();
			var encrypter = MockRepository.GenerateMock<INhibConfigurationEncryption>();
			encrypter.Stub(x => x.EncryptConfig(expected)).Return(expected);
			encrypter.Stub(x => x.EncryptConfig(expectedTwo)).Return(expectedTwo);
			readNHibFiles.Expect(x => x.Read()).Return(new Dictionary<string, DataSourceConfiguration> { { "something", expected }, { "somethingElse", expectedTwo } });
			var target = new DataSourceConfigurationProviderUsingNhibFiles(readNHibFiles,encrypter);
			target.ForTenant(new Tenant("something"))
				.Should().Be.SameInstanceAs(expected);

			target.ForTenant(new Tenant("somethingElse"))
				.Should().Be.SameInstanceAs(expectedTwo);
		}
	}
}