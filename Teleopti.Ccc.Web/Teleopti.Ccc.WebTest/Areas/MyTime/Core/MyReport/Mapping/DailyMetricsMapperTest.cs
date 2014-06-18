using System;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.MyReport.Mapping
{
	[TestFixture]
	public class DailyMetricsMapperTest
	{
		private DailyMetricsMapper _target;
		private IPermissionProvider _permissionProvider;
		private IToggleManager _toggleManager;

		[SetUp]
		public void Setup()
		{
			var culture = CultureInfo.GetCultureInfo("sv-SE");
			var userCulture = MockRepository.GenerateMock<IUserCulture>();
			userCulture.Expect(x => x.GetCulture()).Return(culture);
			_permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			_toggleManager = MockRepository.GenerateMock<IToggleManager>();

			_target = new DailyMetricsMapper(userCulture, _permissionProvider, _toggleManager);
		}

		[Test]
		public void ShouldMapAnsweredCalls()
		{
			var dataModel = new DailyMetricsForDayResult {AnsweredCalls = 55};
			var viewModel = _target.Map(dataModel);

			viewModel.AnsweredCalls.Should().Be.EqualTo(dataModel.AnsweredCalls);
		}

		[Test]
		public void ShouldMapAfterCallWorkTimeAverage()
		{
			var dataModel = new DailyMetricsForDayResult { AfterCallWorkTimeAverage = new TimeSpan(0,0,40) };
			var viewModel = _target.Map(dataModel);

			viewModel.AverageAfterCallWork.Should().Be.EqualTo("40");
		}

		[Test]
		public void ShouldMapTalkTimeAverage()
		{
			var dataModel = new DailyMetricsForDayResult { TalkTimeAverage = new TimeSpan(0, 0, 110) };
			var viewModel = _target.Map(dataModel);

			viewModel.AverageTalkTime.Should().Be.EqualTo("110");
		}

		[Test]
		public void ShouldMapHandlingTimeAverage()
		{
			var dataModel = new DailyMetricsForDayResult { HandlingTimeAverage = new TimeSpan(0, 0, 120),  TalkTimeAverage = new TimeSpan(0, 0, 110), AfterCallWorkTimeAverage = new TimeSpan(0, 0, 5)};
			var viewModel = _target.Map(dataModel);

			viewModel.AverageHandlingTime.Should().Be.EqualTo("115");
		}

		[Test]
		public void ShouldMapReadyTimePerScheduledReadyTime()
		{
			var dataModel = new DailyMetricsForDayResult { ReadyTimePerScheduledReadyTime = new Percent(0.754) };
			var viewModel = _target.Map(dataModel);

			viewModel.ReadyTimePerScheduledReadyTime.Should().Be.EqualTo("75,4");
		}

		[Test]
		public void ShouldMapAdherence()
		{
			var dataModel = new DailyMetricsForDayResult { Adherence = new Percent(0.803) };
			var viewModel = _target.Map(dataModel);

			viewModel.Adherence.Should().Be.EqualTo("80,3");
		}

		[Test]
		public void ShouldSetDataAvailableToFalseWhenDataIsUnavailable()
		{
			var viewModel = _target.Map(null);

			viewModel.DataAvailable.Should().Be.False();
		}

		[Test]
		public void ShouldSetDataAvailableToTrueWhenDataIsAvailable()
		{
			var viewModel = _target.Map(new DailyMetricsForDayResult());

			viewModel.DataAvailable.Should().Be.True();
		}

		[Test]
		public void ShouldSetQueueMetricsEnabledTrue()
		{
			_permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.MyReportQueueMetrics)).Return(true);
			_toggleManager.Stub(x => x.IsEnabled(Toggles.MyReport_AgentQueueMetrics_22254)).Return(true);
			var viewModel = _target.Map(new DailyMetricsForDayResult());

			viewModel.QueueMetricsEnabled.Should().Be.True();
		}

		[Test]
		public void ShouldSetQueueMetricsEnabledFalseWithoutPermission()
		{
			_permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.MyReportQueueMetrics)).Return(false);
			_toggleManager.Stub(x => x.IsEnabled(Toggles.MyReport_AgentQueueMetrics_22254)).Return(true);
			var viewModel = _target.Map(new DailyMetricsForDayResult());

			viewModel.QueueMetricsEnabled.Should().Be.False();
		}

		[Test]
		public void ShouldSetQueueMetricsEnabledFalseWhenToggleOff()
		{
			_permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.MyReportQueueMetrics)).Return(true);
			_toggleManager.Stub(x => x.IsEnabled(Toggles.MyReport_AgentQueueMetrics_22254)).Return(false);
			var viewModel = _target.Map(new DailyMetricsForDayResult());

			viewModel.QueueMetricsEnabled.Should().Be.False();
		}

	}
}
