using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere
{
	[TestFixture]
	public class ReportItemProviderTest
	{
		private IReportsProvider _reportsProvider;
		private IReportUrl _matrixWebsiteUrl;
		
		[SetUp]
		public void Setup()
		{
			_reportsProvider = MockRepository.GenerateMock<IReportsProvider>();
			_matrixWebsiteUrl = MockRepository.GenerateMock<IReportUrl>();
			}

		[Test]
		public void ShouldGetReportItems()
		{
			_reportsProvider.Stub(x => x.GetReports()).Return(new List<IApplicationFunction>() {new ApplicationFunction()});

			var target = new ReportItemsProvider(_reportsProvider, _matrixWebsiteUrl);
			var result = target.GetReportItems();

			result.Count.Should().Be(1);
		}

		[Test]
		public void ShouldGetReportItemNameAndUrl()
		{
			const string localizedFunctionDescription = "report1";

			var applicationFunciton = MockRepository.GenerateMock<IApplicationFunction>();
			applicationFunciton.Stub(x => x.LocalizedFunctionDescription).Return(localizedFunctionDescription);
			_reportsProvider.Stub(x => x.GetReports()).Return(new List<IApplicationFunction>() { applicationFunciton });
			var matrixUrl = "Selection.aspx?ReportId=foreignId&BuId=00000001";
			_matrixWebsiteUrl.Stub(x => x.Build(applicationFunciton.ForeignId)).Return(matrixUrl);

			var target = new ReportItemsProvider(_reportsProvider, _matrixWebsiteUrl);
			var result = target.GetReportItems();

			result[0].Name.Should().Be.EqualTo(localizedFunctionDescription);
			result[0].Url.Should().Be.EqualTo(matrixUrl);
		}

		[Test]
		public void ShouldSortReportItemsByName()
		{
			const string localizedFunctionDescription1 = "report1";
			const string localizedFunctionDescription2 = "report2";

			var applicationFunciton1 = MockRepository.GenerateMock<IApplicationFunction>();
			applicationFunciton1.Stub(x => x.LocalizedFunctionDescription).Return(localizedFunctionDescription1);			
			var applicationFunciton2 = MockRepository.GenerateMock<IApplicationFunction>();
			applicationFunciton2.Stub(x => x.LocalizedFunctionDescription).Return(localizedFunctionDescription2);

			var applicationFunctions = new List<IApplicationFunction>();
			applicationFunctions.Add(applicationFunciton2);
			applicationFunctions.Add(applicationFunciton1);
			_reportsProvider.Stub(x => x.GetReports()).Return(applicationFunctions);

			var target = new ReportItemsProvider(_reportsProvider, _matrixWebsiteUrl);
			var result = target.GetReportItems();

			result[0].Name.Should().Be.EqualTo(localizedFunctionDescription1);
			result[1].Name.Should().Be.EqualTo(localizedFunctionDescription2);
		}
	}
}