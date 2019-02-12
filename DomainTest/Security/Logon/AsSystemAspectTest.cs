using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Security.Logon
{
	[DomainTest]
	[LoggedOff]
	[RealPermissions]
	public class AsSystemAspectTest : IExtendSystem
	{
		public FakeDatabase Database;
		public Service TheService;
		public ICurrentTeleoptiPrincipal Principal;
		public ICurrentAuthorization Authorization;
		public IDefinedRaptorApplicationFunctionFactory ApplicationFunctions;
		public IUserTimeZone TimeZone;
		public IUserCulture Culture;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<Service>();
		}

		public class Service
		{
			[AsSystem]
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
		public void ShouldSignInAsSystem()
		{
			ITeleoptiPrincipal ranWithPrincipal = null;
			var businessUnid = Guid.NewGuid();
			Database
				.WithTenant("tenant")
				.WithBusinessUnit(businessUnid);

			TheService.Do(
				new Input {LogOnDatasource = "tenant", LogOnBusinessUnitId = businessUnid},
				() =>
				{
					ranWithPrincipal = Principal.Current();
				});

			ranWithPrincipal.Identity.Name.Should().Contain("System");
		}

		[Test]
		public void ShouldHaveUtcTimeZone()
		{
			var businessUnid = Guid.NewGuid();
			Database
				.WithTenant("tenant")
				.WithBusinessUnit(businessUnid);
			TimeZoneInfo timeZone = null;

			TheService.Do(new Input {LogOnDatasource = "tenant", LogOnBusinessUnitId = businessUnid},
				() =>
				{
					timeZone = TimeZone.TimeZone();
				});

			timeZone.Should().Be.EqualTo(TimeZoneInfo.Utc);
		}

		[Test]
		public void ShouldHaveEnglishCulture()
		{
			var businessUnid = Guid.NewGuid();
			Database
				.WithTenant("tenant")
				.WithBusinessUnit(businessUnid);
			CultureInfo culture = null;

			TheService.Do(new Input { LogOnDatasource = "tenant", LogOnBusinessUnitId = businessUnid },
				() =>
				{
					culture = Culture.GetCulture();
				});

			culture.Should().Be.EqualTo(CultureInfoFactory.CreateEnglishCulture());
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
						.Where(f => Authorization.Current().IsPermitted(f))
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
					grantedFunctions = Authorization.Current().GrantedFunctions();
				});

			grantedFunctions.Should().Have.SameValuesAs(ApplicationFunctions.ApplicationFunctions);
		}

		[Test, Ignore("todo")]
		public void ShouldSignOut()
		{
			var businessUnid = Guid.NewGuid();
			Database
				.WithTenant("tenant")
				.WithBusinessUnit(businessUnid);

			TheService.Do(new Input {LogOnDatasource = "tenant", LogOnBusinessUnitId = businessUnid}, () => { });

			Principal.Current().Should().Be.Null();
		}

	}
}
