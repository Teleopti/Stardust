using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Tenant.Core;

namespace Teleopti.Ccc.WebTest.Areas.Tenant.Core
{
	public class DataSourceConfigurationProviderUsingNhibFilesTest
	{
		[Test]
		public void ExistingConfiguration()
		{
			var expected = new DataSourceConfiguration();
			var readNHibFiles = MockRepository.GenerateMock<IReadNHibFiles>();
			readNHibFiles.Expect(x => x.Read()).Return(new Dictionary<string, DataSourceConfiguration> {{"something", expected}});
			var target = new DataSourceConfigurationProviderUsingNhibFiles(readNHibFiles);
			target.ForTenant("something")
				.Should().Be.SameInstanceAs(expected);
		}

		[Test]
		public void NonExistingConfiguration()
		{
			var readNHibFiles = MockRepository.GenerateMock<IReadNHibFiles>();
			readNHibFiles.Expect(x => x.Read()).Return(new Dictionary<string, DataSourceConfiguration>());
			var target = new DataSourceConfigurationProviderUsingNhibFiles(readNHibFiles);
			target.ForTenant("notsomething")
				.Should().Be.Null();
		}

		[Test]
		public void ShouldKeepStateIfReadNhibFiles()
		{
			var readNHibFiles = MockRepository.GenerateMock<IReadNHibFiles>();
			readNHibFiles.Expect(x => x.Read()).Return(new Dictionary<string, DataSourceConfiguration>());

			var target = new DataSourceConfigurationProviderUsingNhibFiles(readNHibFiles);
			target.ForTenant("1");
			target.ForTenant("1");
			target.ForTenant("2");

			readNHibFiles.AssertWasCalled(x => x.Read(), x => x.Repeat.Once());
		}
	}
}