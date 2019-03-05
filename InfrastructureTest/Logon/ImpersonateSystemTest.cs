using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Logon
{
	[DatabaseTest]
	public class ImpersonateSystemTest : IExtendSystem
	{
		public Service TheService;
		public ILogOnOffContext Context;
		public IBusinessUnitRepository BusinessUnits;
		public IScenarioRepository Scenarios;
		public WithUnitOfWork UnitOfWork;
		public FakeMessageSender Messages;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<Service>();
		}

		public class Service
		{
			[ImpersonateSystem, UnitOfWork]
			public virtual void Do(Input input, Action action)
			{
				action.Invoke();
			}

		}

		public class Input : ILogOnContext
		{
			public string LogOnDatasource { get; set; }
			public Guid LogOnBusinessUnitId { get; set; }
		}
		
		[Test]
		public void ShouldAddEntityWithBusinessUnit()
		{
			IBusinessUnit entityGotBusinessUnit = null;
			var businessUnit = UnitOfWork.Get(() => BusinessUnits.LoadAll().First());
			Context.Logout();

			TheService.Do(
				new Input { LogOnDatasource = SetupFixtureForAssembly.DataSource.DataSourceName, LogOnBusinessUnitId = businessUnit.Id.Value },
				() =>
				{
					var scenario = new Domain.Common.Scenario("s");
					Scenarios.Add(scenario);
					entityGotBusinessUnit = scenario.BusinessUnit;
				});

			entityGotBusinessUnit.Id.Should().Be(businessUnit.Id);
		}

		[Test]
		public void ShouldAddEntityAsSystem()
		{
			IScenario entityAdded = null;
			var businessUnit = UnitOfWork.Get(() => BusinessUnits.LoadAll().First());
			Context.Logout();

			TheService.Do(
				new Input { LogOnDatasource = SetupFixtureForAssembly.DataSource.DataSourceName, LogOnBusinessUnitId = businessUnit.Id.Value },
				() =>
				{
					entityAdded = new Domain.Common.Scenario("s");
					Scenarios.Add(entityAdded);
				});

			entityAdded.UpdatedBy.Id.Should().Be(SystemUser.Id);
		}

		[Test]
		public void ShouldFindEntityFilteredOnBusinessUnit()
		{
			IScenario entityAdded = null;
			IScenario entityFound = null;
			IBusinessUnit businessUnit = null;
			UnitOfWork.Do(() =>
			{
				entityAdded = new Domain.Common.Scenario("s");
				Scenarios.Add(entityAdded);
				businessUnit = BusinessUnits.LoadAll().First();
			});
			Context.Logout();

			TheService.Do(
				new Input { LogOnDatasource = SetupFixtureForAssembly.DataSource.DataSourceName, LogOnBusinessUnitId = businessUnit.Id.Value },
				() =>
				{
					entityFound = Scenarios.LoadAll().Single();
				});

			entityFound.Should().Be(entityAdded);
		}

		[Test]
		public void ShouldSendMessageWithValues()
		{
			var businessUnit = UnitOfWork.Get(() => BusinessUnits.LoadAll().First());
			Context.Logout();

			TheService.Do(
				new Input { LogOnDatasource = SetupFixtureForAssembly.DataSource.DataSourceName, LogOnBusinessUnitId = businessUnit.Id.Value },
				() =>
				{
					Scenarios.Add(new Domain.Common.Scenario("s"));
				});

			var message = Messages.NotificationsOfDomainType<IScenario>().Single();
			message.DataSource.Should().Be(SetupFixtureForAssembly.DataSource.DataSourceName);
			message.BusinessUnitId.Should().Be(businessUnit.Id.Value.ToString());
		}
	}
}