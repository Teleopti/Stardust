using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
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

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<Service>();
		}

		public class Service
		{
			private readonly ICurrentTeleoptiPrincipal _principal;
			public ITeleoptiPrincipal RanWithPrincipal;

			public Service(ICurrentTeleoptiPrincipal principal)
			{
				_principal = principal;
			}

			[AsSystem]
			public virtual void Call(Input input)
			{
				RanWithPrincipal = _principal.Current();
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
			var businessUnid = Guid.NewGuid();
			Database
				.WithTenant("tenant")
				.WithBusinessUnit(businessUnid);

			TheService.Call(new Input {LogOnDatasource = "tenant", LogOnBusinessUnitId = businessUnid});

			TheService.RanWithPrincipal.Identity.Name.Should().Be("System");
		}

	}
}
