using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Logon
{
	[DomainTest]
	[LoggedOff]
	public class AsSuperUserTest
	{
		public FakeDatabase Database;
		public AsSuperUser Target;
		public ICurrentTeleoptiPrincipal Principal;

		[Test, Ignore]
		public void Should()
		{
			var businessUnid = Guid.NewGuid();
			Database
				.WithTenant("tenant")
				.WithBusinessUnit(businessUnid);

			Target.Logon("tenant", businessUnid);

			Principal.Current().Identity.Name.Should().Be("SuperUser");
		}
	}
}
