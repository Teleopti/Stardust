using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

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

		[Test, SetCulture("sv-SE")]
		public void ShouldReturnTimeSeries()
		{
			var minutesPerInterval = 15;
			_firstInterval = new StaffingIntervalModel {StartTime = DateTime.MinValue.AddMinutes(minutesPerInterval*4) };
			_secondInterval = new StaffingIntervalModel {StartTime = DateTime.MinValue.AddMinutes(minutesPerInterval*5) };

			ForecastedStaffingLoader.AddInterval(_firstInterval);
			ForecastedStaffingLoader.AddInterval(_secondInterval);
			IntervalLengthFetcher.Has(minutesPerInterval);

			var viewModel = Target.Load(new[] { Guid.NewGuid() });

			viewModel.DataSeries.Time.Length.Should().Be.EqualTo(2);
			viewModel.DataSeries.Time.First().Should().Be.EqualTo(_firstInterval.StartTime);
			viewModel.DataSeries.Time.Second().Should().Be.EqualTo(_secondInterval.StartTime);
		}
	}
}