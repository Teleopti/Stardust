using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Web.Core.Licensing;

namespace Teleopti.Ccc.WebTest.Core.Licensing
{
	public class LicenseActivatorReadFromQueryStringTest
	{
		[Test]
		public void ShouldReturnLicenseActivator()
		{
			const string datasourceName ="ds";

			var licenseActivator = MockRepository.GenerateMock<ILicenseActivator>();
			var querystringReader = MockRepository.GenerateMock<IQueryStringReader>();
			var checkLicense = MockRepository.GenerateMock<ICheckLicenseExists>();
			querystringReader.Stub(x => x.GetValue(LicenseActivatorReadFromQueryString.QuerystringKey)).Return(datasourceName);


			DefinedLicenseDataFactory.SetLicenseActivator(datasourceName, licenseActivator);

			var target = new LicenseActivatorReadFromQueryString(querystringReader, checkLicense);

			target.Current()
				.Should().Be.SameInstanceAs(licenseActivator);

			DefinedLicenseDataFactory.ClearLicenseActivators();
		}

		[Test]
		public void ShouldCheckLicense()
		{
			const string datasourceName = "ds";

			var licenseActivator = MockRepository.GenerateMock<ILicenseActivator>();
			var querystringReader = MockRepository.GenerateMock<IQueryStringReader>();
			var checkLicense = MockRepository.GenerateMock<ICheckLicenseExists>();
			querystringReader.Stub(x => x.GetValue(LicenseActivatorReadFromQueryString.QuerystringKey)).Return(datasourceName);


			DefinedLicenseDataFactory.SetLicenseActivator(datasourceName, licenseActivator);

			var target = new LicenseActivatorReadFromQueryString(querystringReader, checkLicense);
			target.Current();

			checkLicense.AssertWasCalled(x => x.Check(datasourceName));

			DefinedLicenseDataFactory.ClearLicenseActivators();
		}
	}
}