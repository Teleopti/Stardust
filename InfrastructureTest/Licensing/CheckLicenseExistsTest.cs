using System;
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
	public class CheckLicenseExistsTest
	{
		[Test]
		public void ShouldThrowIfNoLicenseExists()
		{
			DefinedLicenseDataFactory.ClearLicenseActivators();
			Assert.Throws<DataSourceException>(() => new CheckLicenseExists().Check("asdf")).ToString()
				.Should().Contain(CheckLicenseExists.ErrorMessageIfNoLicenseAtAll);
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

			Assert.Throws<DataSourceException>(() => new CheckLicenseExists().Check("THISONE")).ToString()
				.Should().Contain(string.Format(CheckLicenseExists.ErrorMessageIfNoLicenseForDataSource, "THISONE"));

			DefinedLicenseDataFactory.ClearLicenseActivators();
		}
	}
}