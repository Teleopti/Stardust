using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Licensing
{
	public class LicenseActivatorProviderTest
	{
		[Test]
		public void ShouldGiveCurrentLicenseActivator()
		{
			var licenseActivator = MockRepository.GenerateMock<ILicenseActivator>();

			//setup
			DefinedLicenseDataFactory.SetLicenseActivator("fortest", licenseActivator);
			var uowCurrFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var checkLicense = MockRepository.GenerateMock<ICheckLicenseExists>();
			uowFactory.Expect(x => x.Name).Return("fortest");
			uowCurrFactory.Expect(x => x.LoggedOnUnitOfWorkFactory()).Return(uowFactory);

			var target = new LicenseActivatorProvider(uowCurrFactory, checkLicense);


			target.Current().Should().Be.SameInstanceAs(licenseActivator);
			DefinedLicenseDataFactory.ClearLicenseActivators();
		}

		[Test]
		public void ShouldCheckLicense()
		{
			const string datasourceName = "fortest";

			var licenseActivator = MockRepository.GenerateMock<ILicenseActivator>();
			var uowCurrFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var checkLicense = MockRepository.GenerateMock<ICheckLicenseExists>();
			uowFactory.Expect(x => x.Name).Return("fortest");
			uowCurrFactory.Expect(x => x.LoggedOnUnitOfWorkFactory()).Return(uowFactory);

			DefinedLicenseDataFactory.SetLicenseActivator(datasourceName, licenseActivator);

			var target = new LicenseActivatorProvider(uowCurrFactory, checkLicense);
			target.Current();

			checkLicense.AssertWasCalled(x => x.Check(datasourceName));

			DefinedLicenseDataFactory.ClearLicenseActivators();
		}
	}
}