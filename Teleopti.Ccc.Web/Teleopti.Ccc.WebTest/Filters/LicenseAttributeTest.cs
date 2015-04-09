using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.WebTest.Filters
{
	[TestFixture]
	public class LicenseAttributeTest
	{
		[Test]
		public void ShouldPassThroughWhenPermission()
		{
			var licenseActivator = MockRepository.GenerateMock<ILicenseActivator>();
			licenseActivator.Stub(x => x.EnabledLicenseOptionPaths).Return(new[] {DefinedLicenseOptionPaths.TeleoptiCccSmsLink});
			DefinedLicenseDataFactory.SetLicenseActivator("ds1", licenseActivator);
			var target = new LicenseAttribute(DefinedLicenseOptionPaths.TeleoptiCccSmsLink) { CurrentDataSource = new FakeCurrentDatasource("ds1") };

			var result = new FilterTester().InvokeFilter(target);

			result.Should().Be.OfType<EmptyResult>();
			DefinedLicenseDataFactory.ClearLicenseActivators();
		}

		[Test]
		public void ShouldSetErrorWithAjaxRequestWhenNoPermission()
		{
			var target = new LicenseAttribute(DefinedLicenseOptionPaths.TeleoptiCccSmsLink){CurrentDataSource = new FakeCurrentDatasource("ds1")};

			var result = new FilterTester().InvokeFilter(target);

			result.Should().Be.OfType<JsonResult>();
		}
	}
}