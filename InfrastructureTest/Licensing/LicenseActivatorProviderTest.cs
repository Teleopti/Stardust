using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;
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
			uowFactory.Expect(x => x.Name).Return("fortest");
			uowCurrFactory.Expect(x => x.LoggedOnUnitOfWorkFactory()).Return(uowFactory);

			var target = new LicenseActivatorProvider(uowCurrFactory);


			target.Current().Should().Be.SameInstanceAs(licenseActivator);
			DefinedLicenseDataFactory.ClearLicenseActivators();
		}

		[Test]
		public void ShouldThrowIfNoLicenseExists()
		{
			DefinedLicenseDataFactory.ClearLicenseActivators();
			Assert.Throws<DataSourceException>(() => new LicenseActivatorProvider(null).Current()).ToString()
				.Should().Contain(LicenseActivatorProvider.ErrorMessageIfNoLicenseAtAll);
		}

		[Test]
		public void ShouldThrowIfDataSourceHasNoLicense()
		{
			var licenseActivator = MockRepository.GenerateMock<ILicenseActivator>();

			//setup
			DefinedLicenseDataFactory.SetLicenseActivator("fortest", licenseActivator);
			var uowCurrFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			uowFactory.Expect(x => x.Name).Return("THISONE");
			uowCurrFactory.Expect(x => x.LoggedOnUnitOfWorkFactory()).Return(uowFactory);

			Assert.Throws<DataSourceException>(() => new LicenseActivatorProvider(uowCurrFactory).Current()).ToString()
				.Should().Contain(string.Format(LicenseActivatorProvider.ErrorMessageIfNoLicenseForDataSource, "THISONE"));

			DefinedLicenseDataFactory.ClearLicenseActivators();
		}
	}
}