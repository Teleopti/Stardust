using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[DomainTest]
	public class ForecastedStaffingProviderTest
	{
		public ForecastedStaffingProvider Target;
		public FakeForecastedStaffingLoader ForecastedStaffingLoader;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;

		private StaffingIntervalModel _firstInterval;
		private StaffingIntervalModel _secondInterval;
		private const int minutesPerInterval = 15;

		[SetUp]
		public void Setup()
		{
			_firstInterval = new StaffingIntervalModel
			{
				StartTime = DateTime.MinValue.AddMinutes(minutesPerInterval * 4),
				ForecastedStaffing = 5.7
			};
			_secondInterval = new StaffingIntervalModel
			{
				StartTime = DateTime.MinValue.AddMinutes(minutesPerInterval * 5),
				ForecastedStaffing = 3.4
			};
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldReturnTimeSeries()
		{
			ForecastedStaffingLoader.AddInterval(_firstInterval);
			ForecastedStaffingLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.Load(new[] { Guid.NewGuid() });

			viewModel.DataSeries.Time.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.Time.First().Should().Be.EqualTo(_firstInterval.StartTime);
			viewModel.DataSeries.Time.Second().Should().Be.EqualTo(_secondInterval.StartTime);
		}

		[Test]
		public void ShouldReturnStaffingNumbers()
		{
			ForecastedStaffingLoader.AddInterval(_firstInterval);
			ForecastedStaffingLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.Load(new[] { Guid.NewGuid() });

			viewModel.DataSeries.ForecastedStaffing.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.ForecastedStaffing.First().Should().Be.EqualTo(_firstInterval.ForecastedStaffing);
			viewModel.DataSeries.ForecastedStaffing.Second().Should().Be.EqualTo(_secondInterval.ForecastedStaffing);
		}
	}
}