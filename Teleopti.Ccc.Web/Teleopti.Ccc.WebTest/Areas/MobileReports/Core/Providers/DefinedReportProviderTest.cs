using System.Linq;
using MvcContrib.TestHelper.Fakes;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MobileReports.Core;
using Teleopti.Ccc.Web.Areas.MobileReports.Core.Providers;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.WebTest.Areas.MobileReports.TestData;

namespace Teleopti.Ccc.WebTest.Areas.MobileReports.Core.Providers
{
	[TestFixture]
	public class DefinedReportProviderTest
	{

		[Test]
		public void ShouldFilterReportsByFunctionsCode()
		{
			const string reportId = "GetForeCastVsActualWorkload";
			var excludeFunction = DefinedReports.ReportInformations.First(x => reportId.Equals(x.ReportId)).FunctionCode;
			var applicationFunctions = new DefinedReportsApplicationFunctionsFactory(excludeFunction).ApplicationFunctions;
			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			var target = new DefinedReportProvider(principalAuthorization);

			principalAuthorization.Stub(x => x.GrantedFunctionsBySpecification(null)).IgnoreArguments().Return(applicationFunctions);

			var definedReportInformations = target.GetDefinedReports();

			definedReportInformations.Count().Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldReturnDefinedReportById()
		{
			const string reportId = "GetForeCastVsActualWorkload";
			var applicationFunctions = new DefinedReportsApplicationFunctionsFactory(null).ApplicationFunctions;
			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			var target = new DefinedReportProvider(principalAuthorization);

			principalAuthorization.Stub(x => x.GrantedFunctionsBySpecification(null)).IgnoreArguments().Return(applicationFunctions);

			var report = target.Get(reportId);

			report.ReportId.Should().Be.EqualTo(reportId);
		}

		[Test]
		public void ShouldReturnDefinedReports()
		{
			var applicationFunctions = new DefinedReportsApplicationFunctionsFactory(null).ApplicationFunctions;
			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			var target = new DefinedReportProvider(principalAuthorization);

			principalAuthorization.Stub(x => x.GrantedFunctionsBySpecification(null)).IgnoreArguments().Return(applicationFunctions);

			var definedReportInformations = target.GetDefinedReports();

			definedReportInformations.Count().Should().Be.EqualTo(4);
		}

		[Test]
		public void ShouldReturnNullIfDefinedReportNotFound()
		{
			const string reportId = "NonExistent";

			var applicationFunctions = new DefinedReportsApplicationFunctionsFactory(null).ApplicationFunctions;
			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			var target = new DefinedReportProvider(principalAuthorization);

			principalAuthorization.Stub(x => x.GrantedFunctionsBySpecification(null)).IgnoreArguments().Return(applicationFunctions);

			var report = target.Get(reportId);

			report.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullIfReportFunctionCodeIsNotPermitted()
		{
			const string reportId = "GetForeCastVsActualWorkload";

			var excludeFunction = DefinedReports.ReportInformations.First(x => reportId.Equals(x.ReportId)).FunctionCode;
			var applicationFunctions = new DefinedReportsApplicationFunctionsFactory(excludeFunction).ApplicationFunctions;
			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			var target = new DefinedReportProvider(principalAuthorization);

			principalAuthorization.Stub(x => x.GrantedFunctionsBySpecification(null)).IgnoreArguments().Return(applicationFunctions);

			var report = target.Get(reportId);

			report.Should().Be.Null();
		}
	}
}