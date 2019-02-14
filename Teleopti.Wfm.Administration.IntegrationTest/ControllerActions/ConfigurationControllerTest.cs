using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[WfmAdminTest]
	public class ConfigurationControllerTest
	{
		public ConfigurationController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		public IServerConfigurationRepository ServerConfigurationRepository;

		[Test]
		public void GetAllShouldContainFrameAncestors()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			var configurationModels = Target.GetAllConfigurations().Content;
			configurationModels.SingleOrDefault(x => x.Key == "FrameAncestors").Should().Not.Be.Null();
			configurationModels.SingleOrDefault(x => x.Key == "InstrumentationKey").Should().Not.Be.Null();
		}

		[Test]
		public void ShouldGetFrameAncestors()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			var value = Target.GetConfiguration("FrameAncestors").Content;
			value.Should().Be.EqualTo("");
		}

		[Test]
		public void ShouldGetInstrumentationKey()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			var value = Target.GetConfiguration("InstrumentationKey").Content;
			value.Should().Be.EqualTo("");
		}

		[Test]
		public void ShouldGetLogonAttemptsDays()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			var configurationModels = Target.GetAllConfigurations().Content;
			var value = configurationModels.SingleOrDefault(x => x.Key == "PreserveLogonAttemptsDays");
			value.Should().Not.Be.Null();
			value.Value.Should().Be.EqualTo("30");
		}

		[Test]
		public void ShouldSave()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());

			var updateConfigurationModel = new UpdateConfigurationModel
			{
				Key = "FrameAncestors",
				Value = "http://localhost1"
			};
			var result = Target.Save(updateConfigurationModel);
			result.Success.Should().Be.True();
			var value = Target.GetConfiguration(updateConfigurationModel.Key).Content;
			value.Should().Be.EqualTo(updateConfigurationModel.Value);
		}
	}
}