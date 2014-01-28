using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.MyReport.Mapping
{
	[TestFixture]
	public class DailyMetricsMapperTest
	{
		[SetUp]
		public void Setup()
		{
			
		}

		[Test]
		public void ShouldMapAnsweredCalls()
		{
			var target = new DailyMetricsMapper();
			var dataModel = new DailyMetricsForDayResult {AnsweredCalls = 55};
			var viewModel = target.Map(dataModel);

			viewModel.AnsweredCalls.Should().Be.EqualTo(dataModel.AnsweredCalls)
		}
	}
}
