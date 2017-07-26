using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class ConfigurationControllerTest
	{
		public ConfigurationController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		public IServerConfigurationRepository ServerConfigurationRepository;

		[Test]
		public void GetAllShouldContainFrameAncestors()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(new NoTransactionHooks(), "TestData");
			var configurationModels = Target.GetAllConfigurations().Content;
			configurationModels.First().Key.Should().Be.EqualTo("FrameAncestors");
			configurationModels.First().Value.Should().Be.EqualTo("");
			configurationModels.Last().Key.Should().Be.EqualTo("InstrumentationKey");
			configurationModels.Last().Value.Should().Be.EqualTo("");
		}

		[Test]
		public void ShouldGetFrameAncestors()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(new NoTransactionHooks(), "TestData");
			var value = Target.GetConfiguration("FrameAncestors").Content;
			value.Should().Be.EqualTo("");
		}

		[Test]
		public void ShouldGetInstrumentationKey()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(new NoTransactionHooks(), "TestData");
			var value = Target.GetConfiguration("InstrumentationKey").Content;
			value.Should().Be.EqualTo("");
		}

		[Test]
		public void ShouldSave()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(new NoTransactionHooks(), "TestData");

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