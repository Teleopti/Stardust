using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[TestFixture]
	public class ReportItemProviderTest
	{
		private IReportsProvider _reportsProvider;
		private ICurrentBusinessUnit _currentBusinessUnit;
		private IMatrixWebsiteUrl _matrixWebsiteUrl;
		private Guid _guid;

		[SetUp]
		public void Setup()
		{
			_guid = Guid.NewGuid();

			_reportsProvider = MockRepository.GenerateMock<IReportsProvider>();
			_currentBusinessUnit = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			_matrixWebsiteUrl = MockRepository.GenerateMock<IMatrixWebsiteUrl>();
			_currentBusinessUnit.Stub(x => x.Current().Id).Return(_guid);
		}

		[Test]
		public void ShouldGetReportItems()
		{
			_reportsProvider.Stub(x => x.GetReports()).Return(new List<IApplicationFunction>() {new ApplicationFunction()});

			var target = new ReportItemsProvider(_reportsProvider, _currentBusinessUnit, _matrixWebsiteUrl);
			var result = target.GetReportItems();

			result.Count.Should().Be(1);
		}

		[Test]
		public void ShouldGetReportItemNameAndUrl()
		{
			const string localizedFunctionDescription = "report1";

			var applicationFunciton = MockRepository.GenerateMock<IApplicationFunction>();
			applicationFunciton.Stub(x => x.LocalizedFunctionDescription).Return(localizedFunctionDescription);
			applicationFunciton.Stub(y => y.ForeignId).Return("foreignId");
			_reportsProvider.Stub(x => x.GetReports()).Return(new List<IApplicationFunction>() { applicationFunciton });

			var target = new ReportItemsProvider(_reportsProvider, _currentBusinessUnit, _matrixWebsiteUrl);
			var result = target.GetReportItems();

			result[0].Name.Should().Be.EqualTo(localizedFunctionDescription);
			result[0].Url.Should().Be.EqualTo("Selection.aspx?ReportId=foreignId&BuId=" + _guid);
		}

		[Test]
		public void ShouldSeperateWithSlashWhenHasMatrixWebsiteUrl()
		{
			var matrixUrl = "MatrixUrl/";
			_matrixWebsiteUrl.Stub(x => x.Build()).Return(matrixUrl);
			var applicationFunciton = MockRepository.GenerateMock<IApplicationFunction>();
			applicationFunciton.Stub(y => y.ForeignId).Return("foreignId");
			_reportsProvider.Stub(x => x.GetReports()).Return(new List<IApplicationFunction>() { applicationFunciton });

			var target = new ReportItemsProvider(_reportsProvider, _currentBusinessUnit, _matrixWebsiteUrl);
			var result = target.GetReportItems();

			result[0].Url.Should().Be.EqualTo(matrixUrl + "Selection.aspx?ReportId=foreignId&BuId=" + _guid);
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

			var target = new ReportItemsProvider(_reportsProvider, _currentBusinessUnit, _matrixWebsiteUrl);
			var result = target.GetReportItems();

			result[0].Name.Should().Be.EqualTo(localizedFunctionDescription1);
			result[1].Name.Should().Be.EqualTo(localizedFunctionDescription2);
		}
	}
}