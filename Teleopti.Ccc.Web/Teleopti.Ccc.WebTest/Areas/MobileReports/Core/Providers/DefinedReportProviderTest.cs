namespace Teleopti.Ccc.WebTest.Areas.MobileReports.Core.Providers
{
	using System.Linq;

	using MvcContrib.TestHelper.Fakes;

	using NUnit.Framework;

	using Rhino.Mocks;

	using SharpTestsEx;

	using Teleopti.Ccc.Domain.Security.Principal;
	using Teleopti.Ccc.TestCommon;
	using Teleopti.Ccc.Web.Areas.MobileReports.Core;
	using Teleopti.Ccc.Web.Areas.MobileReports.Core.IoC;
	using Teleopti.Ccc.Web.Areas.MobileReports.Core.Providers;
	using Teleopti.Ccc.WebTest.Areas.MobileReports.TestData;

	[TestFixture]
	public class DefinedReportProviderTest
	{
		private MockRepository _mock;

		private IPrincipalAuthorization _principalAuthorization;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_principalAuthorization = _mock.DynamicMock<IPrincipalAuthorization>();
		}

		[Test]
		public void ShouldFilterReportsByFunctionsCode()
		{
			const string reportId = "GetForeCastVsActualWorkload";
			var excludeFunction = DefinedReports.ReportInformations.First(x => reportId.Equals(x.ReportId)).FunctionCode;
			var applicationFunctions = new DefinedReportsApplicationFunctionsFactory(excludeFunction).ApplicationFunctions;
			var target = new DefinedReportProvider(this.CreatePrincipalProvider());

			using (_mock.Record())
			{
				Expect.Call(_principalAuthorization.GrantedFunctionsBySpecification(null)).IgnoreArguments().Return(
					applicationFunctions).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var definedReportInformations = target.GetDefinedReports();
				definedReportInformations.Count().Should().Be.EqualTo(3);
			}
		}

		[Test]
		public void ShouldReturnDefinedReportById()
		{
			const string reportId = "GetForeCastVsActualWorkload";
			var applicationFunctions = new DefinedReportsApplicationFunctionsFactory(null).ApplicationFunctions;
			var target = new DefinedReportProvider(this.CreatePrincipalProvider());

			using (_mock.Record())
			{
				Expect.Call(_principalAuthorization.GrantedFunctionsBySpecification(null)).IgnoreArguments().Return(
					applicationFunctions).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var report = target.Get(reportId);

				report.ReportId.Should().Be.EqualTo(reportId);
			}
		}

		[Test]
		public void ShouldReturnDefinedReports()
		{
			var applicationFunctions = new DefinedReportsApplicationFunctionsFactory(null).ApplicationFunctions;
			var target = new DefinedReportProvider(this.CreatePrincipalProvider());

			using (_mock.Record())
			{
				Expect.Call(_principalAuthorization.GrantedFunctionsBySpecification(null)).IgnoreArguments().Return(
					applicationFunctions).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var definedReportInformations = target.GetDefinedReports();
				definedReportInformations.Count().Should().Be.EqualTo(4);
			}
		}

		[Test]
		public void ShouldReturnNullIfDefinedReportNotFound()
		{
			const string reportId = "NonExistent";

			var applicationFunctions = new DefinedReportsApplicationFunctionsFactory(null).ApplicationFunctions;
			var target = new DefinedReportProvider(this.CreatePrincipalProvider());

			using (_mock.Record())
			{
				Expect.Call(_principalAuthorization.GrantedFunctionsBySpecification(null)).IgnoreArguments().Return(
					applicationFunctions).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var report = target.Get(reportId);

				report.Should().Be.Null();
			}
		}

		[Test]
		public void ShouldReturnNullIfReportFunctionCodeIsNotPermitted()
		{
			const string reportId = "GetForeCastVsActualWorkload";

			var excludeFunction = DefinedReports.ReportInformations.First(x => reportId.Equals(x.ReportId)).FunctionCode;
			var applicationFunctions = new DefinedReportsApplicationFunctionsFactory(excludeFunction).ApplicationFunctions;
			var target = new DefinedReportProvider(this.CreatePrincipalProvider());

			using (_mock.Record())
			{
				Expect.Call(_principalAuthorization.GrantedFunctionsBySpecification(null)).IgnoreArguments().Return(
					applicationFunctions).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var report = target.Get(reportId);

				report.Should().Be.Null();
			}
		}

		private PrincipalProviderForTest CreatePrincipalProvider()
		{
			var teleoptiPrincipalForTest = new TeleoptiPrincipalForTest(new FakeIdentity("MeFaky"), null);
			teleoptiPrincipalForTest.SetPrincipalAuthorization(this._principalAuthorization);
			return new PrincipalProviderForTest(teleoptiPrincipalForTest);
		}
	}
}