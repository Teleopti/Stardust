using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Security.Logon
{
	[DomainTest]
	[LoggedOff]
	[RealPermissions]
	public class ImpersonateSystemTest : IExtendSystem
	{
		public FakeDatabase Database;
		public Service TheService;
		public ICurrentTeleoptiPrincipal Principal;
		public IScenarioRepository Scenarios;
		public ICurrentAuthorization PrincipalAuthorization;
		public IDefinedRaptorApplicationFunctionFactory ApplicationFunctions;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<Service>();
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
					entityGotBusinessUnit = scenario.GetOrFillWithBusinessUnit_DONTUSE();
				});

			entityGotBusinessUnit.Id.Should().Be(businessUnid);
		}

		[Test]
		public void ShouldBePermittedAllFunctions()
		{
			var permitted = Enumerable.Empty<string>();
			var businessUnid = Guid.NewGuid();
			Database
				.WithTenant("tenant")
				.WithBusinessUnit(businessUnid);

			TheService.Do(new Input { LogOnDatasource = "tenant", LogOnBusinessUnitId = businessUnid },
				() =>
				{
					permitted = ApplicationFunctions.ApplicationFunctions
						.Select(f => f.FunctionPath)
						.Where(f => PrincipalAuthorization.Current().IsPermitted(f))
						.ToArray();
				});

			permitted.Should().Have.SameValuesAs(ApplicationFunctions.ApplicationFunctions.Select(x => x.FunctionPath));
		}

		[Test]
		public void ShouldBeGrantedAllFunctions()
		{
			var grantedFunctions = Enumerable.Empty<IApplicationFunction>();
			var businessUnid = Guid.NewGuid();
			Database
				.WithTenant("tenant")
				.WithBusinessUnit(businessUnid);

			TheService.Do(new Input { LogOnDatasource = "tenant", LogOnBusinessUnitId = businessUnid },
				() =>
				{
					grantedFunctions = PrincipalAuthorization.Current().GrantedFunctions();
				});

			grantedFunctions.Should().Have.SameValuesAs(ApplicationFunctions.ApplicationFunctions);
		}
	}
}