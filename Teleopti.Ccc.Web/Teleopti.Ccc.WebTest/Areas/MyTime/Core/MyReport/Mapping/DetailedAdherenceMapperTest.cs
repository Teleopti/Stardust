using System.Drawing;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.MyReport.Mapping
{
	[TestFixture]
	public class DetailedAdherenceMapperTest
	{
		private DetailedAdherenceMapper _target;
		private CultureInfo _culture;

		[SetUp]
		public void Setup()
		{
			_culture = CultureInfo.GetCultureInfo("sv-SE");
			var userCulture = MockRepository.GenerateMock<IUserCulture>();
			userCulture.Expect(x => x.GetCulture()).Return(_culture);

			_target = new DetailedAdherenceMapper(userCulture);
		}

		[Test]
		public void ShouldMapDataAvailable()
		{
			var dataModel = new DetailedAdherenceForDayResult[] {};

			var viewModel = _target.Map(dataModel);

			viewModel.DataAvailable.Should().Be.False();
		}

		[Test]
		public void ShouldMapShiftDateAndTotalAdherenceAndIntervalsPerDay()
		{
			var dataModel = new[]
			{
				new DetailedAdherenceForDayResult
				{
					ShiftDate = DateOnly.Today, 
					TotalAdherence = new Percent(0.548),
					IntervalsPerDay = 96
				}
			};
			var viewModel = _target.Map(dataModel);

			viewModel.ShiftDate.Should().Be.EqualTo(dataModel.First().ShiftDate.ToShortDateString(_culture));
			viewModel.TotalAdherence.Should().Be.EqualTo(dataModel.First().TotalAdherence.ValueAsPercent().ToString(_culture));
			viewModel.IntervalsPerDay.Should().Be.EqualTo(dataModel.First().IntervalsPerDay);
			viewModel.DataAvailable.Should().Be.True();
		}

		[Test]
		public void ShouldMapIntervalId()
		{
			var dataModel = new[]
			{
				new DetailedAdherenceForDayResult
				{
					IntervalId = 3,
					IntervalsPerDay = 96
				}
			};
			var viewModel = _target.Map(dataModel);

			viewModel.Intervals.First().IntervalId.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldMapIntervalCounter()
		{
			var dataModel = new[]
			{
				new DetailedAdherenceForDayResult
				{
					IntervalCounter = 1,
					IntervalsPerDay = 96
				}
			};
			var viewModel = _target.Map(dataModel);

			viewModel.Intervals.First().IntervalCounter.Should().Be.EqualTo(dataModel.First().IntervalCounter);
		}

		[Test]
		public void ShouldMapDeviation()
		{
			var dataModel = new[]
			{
				new DetailedAdherenceForDayResult
				{
					Deviation = 4,
					IntervalsPerDay = 96
				}
			};
			var viewModel = _target.Map(dataModel);

			viewModel.Intervals.First().Deviation.Should().Be.EqualTo(dataModel.First().Deviation);
		}

		[Test]
		public void ShouldMapAdherence()
		{
			var dataModel = new[]
			{
				new DetailedAdherenceForDayResult
				{
					Adherence = 0.6,
					IntervalsPerDay = 96
				}
			};
			var viewModel = _target.Map(dataModel);

			viewModel.Intervals.First().Adherence.Should().Be.EqualTo(dataModel.First().Adherence);
		}

		[Test]
		public void ShouldMapName()
		{
			var dataModel = new[]
			{
				new DetailedAdherenceForDayResult
				{
					DisplayName = "test",
					IntervalsPerDay = 96
				}
			};
			var viewModel = _target.Map(dataModel);

			viewModel.Intervals.First().Name.Should().Be.EqualTo(dataModel.First().DisplayName);
		}

		[Test]
		public void ShouldMapColor()
		{
			var dataModel = new[]
			{
				new DetailedAdherenceForDayResult
				{
					DisplayColor = Color.ForestGreen,
					IntervalsPerDay = 96
				}
			};
			var viewModel = _target.Map(dataModel);

			viewModel.Intervals.First().Color.Should().Be.EqualTo(dataModel.First().DisplayColor.ToHtml());
		}
	}
}