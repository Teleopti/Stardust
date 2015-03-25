using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;

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

		[Test]
		public void ShouldGetReportUrlWithoutMatrixUrl()
		{
			var configReader = new FakeConfigReader();
			
			var target = new ReportUrl(null, _currentBusinessUnit, configReader);
			var result = target.Build(foreignId);

			result.Should().Be("/" + urlPart1 + foreignId + urlPart2 + _guid);
		}

		[Test]
		public void ShouldGetReportUrlWithMatrixUrl()
		{
			var configReader = new FakeConfigReader();
			const string matrixUrl = "MatrixUrl";
			configReader.AppSettings.Add("MatrixWebSiteUrl", matrixUrl);

			var target = new ReportUrl(matrixUrl, _currentBusinessUnit, configReader);
			var result = target.Build(foreignId);

			result.Should().Be(matrixUrl + "/" + urlPart1 + foreignId + urlPart2 + _guid);
		}

		[Test]
		public void ShouldGetRelativeReportUrlWhenRunningWithRelativeConfiguration()
		{
			var configReader = new FakeConfigReader();
			const string matrixUrl = "http://myserver/TeleoptiWFM/Analytics";
			configReader.AppSettings.Add("MatrixWebSiteUrl", matrixUrl);
			configReader.AppSettings.Add("UseRelativeConfiguration", "true");

			var target = new ReportUrl(matrixUrl, _currentBusinessUnit, configReader);
			var result = target.Build(foreignId);

			result.Should().Be("/TeleoptiWFM/Analytics/" + urlPart1 + foreignId + urlPart2 + _guid);
		}		
		
		[Test]
		public void ShouldGetReportUrlWithMatrixUrlEndWithSlash()
		{
			var configReader = new FakeConfigReader();
			const string matrixUrl = "MatrixUrl/";
			configReader.AppSettings.Add("MatrixWebSiteUrl", matrixUrl);

			var target = new ReportUrl(matrixUrl, _currentBusinessUnit, configReader);
			var result = target.Build(foreignId);

			result.Should().Be(matrixUrl + urlPart1 + foreignId + urlPart2 + _guid);
		}
	}
}