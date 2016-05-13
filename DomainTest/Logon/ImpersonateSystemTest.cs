using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.DomainTest.Logon
{
	[DomainTest]
	[LoggedOff]
	[RealPermissions]
	public class ImpersonateSystemTest : ISetup
	{
		public FakeDatabase Database;
		public Service TheService;
		public ICurrentTeleoptiPrincipal Principal;
		public IScenarioRepository Scenarios;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<Service>();
		}

		public class Service
		{
			[ImpersonateSystem]
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
		public void ShouldNotBeSignedIn()
		{
			ITeleoptiPrincipal ranWithPrincipal = null;
			var businessUnitId = Guid.NewGuid();
			Database
				.WithTenant("tenant")
				.WithBusinessUnit(businessUnitId);

			TheService.Do(
				new Input { LogOnDatasource = "tenant", LogOnBusinessUnitId = businessUnitId },
				() =>
				{
					ranWithPrincipal = Principal.Current();
				});

			ranWithPrincipal.Should().Be.Null();
		}

		[Test]
		public void ShouldAddEntityWithBusinessUnit()
		{
			IBusinessUnit entityGotBusinessUnit = null;
			var businessUnid = Guid.NewGuid();
			Database
				.WithTenant("tenant")
				.WithBusinessUnit(businessUnid);

			TheService.Do(
				new Input { LogOnDatasource = "tenant", LogOnBusinessUnitId = businessUnid },
				() =>
				{
					var scenario = new Domain.Common.Scenario("s");
					Scenarios.Add(scenario);
					entityGotBusinessUnit = scenario.BusinessUnit;
				});

			entityGotBusinessUnit.Id.Should().Be(businessUnid);
		}
		
	}
}