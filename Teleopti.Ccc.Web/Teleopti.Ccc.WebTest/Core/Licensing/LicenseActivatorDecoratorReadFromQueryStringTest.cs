using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Web.Core.Licensing;

namespace Teleopti.Ccc.WebTest.Core.Licensing
{
	public class LicenseActivatorDecoratorReadFromQueryStringTest
	{
		[Test]
		public void ShouldReturnLicenseActivator()
		{
			const string datasourceName ="ds";

			var licenseActivator = MockRepository.GenerateMock<ILicenseActivator>();
			var querystringReader = MockRepository.GenerateMock<IQueryStringReader>();
			var checkLicense = MockRepository.GenerateMock<ICheckLicenseExists>();
			querystringReader.Stub(x => x.GetValue(LicenseActivatorDecoratorReadFromQueryString.QuerystringKey)).Return(datasourceName);


			DefinedLicenseDataFactory.SetLicenseActivator(datasourceName, licenseActivator);

			var target = new LicenseActivatorDecoratorReadFromQueryString(null, querystringReader, checkLicense);

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
			querystringReader.Stub(x => x.GetValue(LicenseActivatorDecoratorReadFromQueryString.QuerystringKey)).Return(datasourceName);


			DefinedLicenseDataFactory.SetLicenseActivator(datasourceName, licenseActivator);

			var target = new LicenseActivatorDecoratorReadFromQueryString(null, querystringReader, checkLicense);
			target.Current();

			checkLicense.AssertWasCalled(x => x.Check(datasourceName));

			DefinedLicenseDataFactory.ClearLicenseActivators();
		}

		[Test]
		public void ShouldFallbackToLoggedOnLicenseIfNoQuerystring()
		{
			var querystringReader = MockRepository.GenerateMock<IQueryStringReader>();
			querystringReader.Stub(x => x.GetValue(LicenseActivatorDecoratorReadFromQueryString.QuerystringKey)).Return(null);
			var checkLicense = MockRepository.GenerateMock<ICheckLicenseExists>();
			var defaultLicenseActivatorProvider = MockRepository.GenerateStub<ILicenseActivatorProvider>();
			var licenseActivator = MockRepository.GenerateStub<ILicenseActivator>();
			defaultLicenseActivatorProvider.Stub(x => x.Current()).Return(licenseActivator);
			var target = new LicenseActivatorDecoratorReadFromQueryString(defaultLicenseActivatorProvider, querystringReader, checkLicense);

			target.Current()
				.Should().Be.SameInstanceAs(licenseActivator);
		}
	}
}