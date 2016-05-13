using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.InfrastructureTest.Logon
{
	[PrincipalAndStateTest]
	public class ImpersonateSystemTest : ISetup
	{
		public Service TheService;
		public IPrincipalAndStateContext Context;
		public IBusinessUnitRepository BusinessUnits;
		public IScenarioRepository Scenarios;
		public WithUnitOfWork UnitOfWork;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<Service>();
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
	}
}