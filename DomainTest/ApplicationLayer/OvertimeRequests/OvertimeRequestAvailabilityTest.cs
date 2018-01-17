using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	[DomainTest]
	[TestFixture]
	public class OvertimeRequestAvailabilityTest
	{
		public IOvertimeRequestAvailability Target;
		public ICurrentDataSource CurrentDataSource;
		public FullPermission Authorization;

		[Test]
		public void ShouldGetOvertimeRequestAvailability()
		{
			var result = Target.IsEnabled();

			result.Should().Be.True();
		}

		[Test]
		public void ShouldDisabledOvertimeRequestWhenNoLicense()
		{
			var licenseActivator = LicenseProvider.GetLicenseActivator(new OvertimeFakeLicenseService(true, false));

			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			var result = Target.IsEnabled();

			result.Should().Be.False();
		}

		[Test]
		public void ShouldDisabledOvertimeRequestWhenNoPermission()
		{
			Authorization.AddToBlackList(DefinedRaptorApplicationFunctionPaths.OvertimeRequestWeb);

			var result = Target.IsEnabled();

			result.Should().Be.False();
		}
	}
}
