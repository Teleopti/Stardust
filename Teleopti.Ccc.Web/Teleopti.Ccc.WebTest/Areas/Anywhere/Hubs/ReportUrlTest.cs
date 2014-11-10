using System;
using System.Configuration;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[TestFixture]
	public class ReportUrlTest
	{
		private ICurrentBusinessUnit _currentBusinessUnit;
		private const string urlPart1 = "Selection.aspx?ReportId=";
		private const string urlPart2 = "&BuId=";
		const string foreignId = "foreignId";
		private Guid _guid;

		[SetUp]
		public void Setup()
		{
			_guid = Guid.NewGuid();
			_currentBusinessUnit = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			_currentBusinessUnit.Stub(x => x.Current().Id).Return(_guid);
		}

		[TearDown]
		public void Teardown()
		{
			ConfigurationManager.AppSettings.Set("MatrixWebSiteUrl", "");
		}

		[Test]
		public void ShouldGetReportUrlWithoutMatrixUrl()
		{
			var target = new ReportUrl(_currentBusinessUnit);
			var result = target.Build(foreignId);

			result.Should().Be(urlPart1 + foreignId + urlPart2 + _guid);
		}

		[Test]
		public void ShouldGetReportUrlWithMatrixUrl()
		{
			var matrixUrl = "MatrixUrl";
			ConfigurationManager.AppSettings.Set("MatrixWebSiteUrl", matrixUrl);

			var target = new ReportUrl(_currentBusinessUnit);
			var result = target.Build(foreignId);

			result.Should().Be(matrixUrl + "/" + urlPart1 + foreignId + urlPart2 + _guid);
		}		
		
		[Test]
		public void ShouldGetReportUrlWithMatrixUrlEndWithSlash()
		{
			var matrixUrl = "MatrixUrl/";
			ConfigurationManager.AppSettings.Set("MatrixWebSiteUrl", matrixUrl);

			var target = new ReportUrl(_currentBusinessUnit);
			var result = target.Build(foreignId);

			result.Should().Be(matrixUrl + urlPart1 + foreignId + urlPart2 + _guid);
		}

	}
}