using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	public class NoRtaLicenseTest : ISetup
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeNoRtaLicenseActivatorProvider>().For<ILicenseActivatorProvider>();
		}

		[Test]
		public void ShouldNotCheckActivityChangeWithoutRtaLicense()
		{
			var person = Guid.NewGuid();
			Database
				.WithUser("user", person)
				.WithSchedule(person, Guid.NewGuid(), "Phone", "2016-08-17 10:00", "2016-08-17 16:00");
			Now.Is("2016-08-17 10:00");
			Target.CheckForActivityChanges(Database.TenantName());

			Database.PersistedReadModel.Should().Be.Null();
		}
	}
}
