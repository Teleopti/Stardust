using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NHibernate.Util;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.DomainTest.Logon
{
	[DomainTest]
	[LoggedOff]
	public class AsSystemAspectTest : ISetup
	{
		public FakeDatabase Database;
		public Service TheService;
		public ICurrentTeleoptiPrincipal Principal;
		public IPrincipalAuthorization PrincipalAuthorization;
		public IDefinedRaptorApplicationFunctionFactory ApplicationFunctions;
		public IUserTimeZone TimeZone;
		public IUserCulture Culture;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<Service>();
			system.AddService<PrincipalAuthorization>();
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

			ranWithPrincipal.Identity.Name.Should().Be("System");
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
		public void ShouldHaveAllPermissions()
		{
			var permitted = Enumerable.Empty<string>();
			var businessUnid = Guid.NewGuid();
			Database
				.WithTenant("tenant")
				.WithBusinessUnit(businessUnid);

			TheService.Do(new Input { LogOnDatasource = "tenant", LogOnBusinessUnitId = businessUnid },
				() =>
				{
					permitted = ApplicationFunctions.ApplicationFunctionList
						.Select(f => f.FunctionPath)
						.Where(f => PrincipalAuthorization.IsPermitted(f))
						.ToArray();
				});

			permitted.Should().Have.SameValuesAs(ApplicationFunctions.ApplicationFunctionList.Select(x => x.FunctionPath));
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
