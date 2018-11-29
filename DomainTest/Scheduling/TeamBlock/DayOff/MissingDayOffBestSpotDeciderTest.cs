using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.DayOff;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.DayOff
{
	[TestFixture]
	public class MissingDayOffBestSpotDeciderTest
	{
		private MissingDayOffBestSpotDecider _target;

		[SetUp]
		public void Setup()
		{
			_target = new MissingDayOffBestSpotDecider(new BestSpotForAddingDayOffFinder());
		}

		[Test]
		public void ShouldFindBestSpotIfPossibleAndReturnLastDayOfAWeekIfNoContractSchedule()
		{		
			var workingPeriod = new MatrixDataForTest(createOneWeek());
			var result = _target.Find(workingPeriod, new List<DateOnlyPeriod> {new DateOnlyPeriod(2015, 12, 7, 2015, 12, 13)}, new List<DateOnly>());
			result.Should().Be.EqualTo(new DateOnly(2015, 12, 13));
		}

		[Test]
		public void ShouldReturnBestPossibleAtTheEndOfTheWeekWhenBestIsBlocked()
		{
			var workingPeriod = new MatrixDataForTest(createOneWeek());
			var result = _target.Find(workingPeriod, new List<DateOnlyPeriod> { new DateOnlyPeriod(2015, 12, 7, 2015, 12, 13) }, new List<DateOnly> { new DateOnly(2015, 12, 13) });
			result.Should().Be.EqualTo(new DateOnly(2015, 12, 12));
		}

		[Test]
		public void ShouldReturnNullIfNoDateCouldBeFound()
		{
			var workingPeriod = new MatrixDataForTest(createOneWeek());
			var result = _target.Find(workingPeriod, new List<DateOnlyPeriod> { new DateOnlyPeriod(2015, 12, 7, 2015, 12, 13) }, new DateOnlyPeriod(2015, 12, 7, 2015, 12, 13).DayCollection());
			result.HasValue.Should().Be.False();
		}

		[Test]
		public void ShouldSelectDateFromWeekWithLeastExistingDaysOff()
		{
			var workingPeriod = new MatrixDataForTest(createTwoWeeks());
			workingPeriod.SetDayOff(new DateOnly(2015, 12, 20));
			var result = _target.Find(workingPeriod, new List<DateOnlyPeriod> { new DateOnlyPeriod(2015, 12, 7, 2015, 12, 13), new DateOnlyPeriod(2015, 12, 14, 2015, 12, 20) }, new List<DateOnly>());
			result.Should().Be.EqualTo(new DateOnly(2015, 12, 13));
		}

		[Test]
		public void ShouldSelectDateFromLastWeekIfEqualDaysOff()
		{
			var workingPeriod = new MatrixDataForTest(createTwoWeeks());
			workingPeriod.SetDayOff(new DateOnly(2015, 12, 20));
			workingPeriod.SetDayOff(new DateOnly(2015, 12, 12));
			var result = _target.Find(workingPeriod, new List<DateOnlyPeriod> { new DateOnlyPeriod(2015, 12, 7, 2015, 12, 13), new DateOnlyPeriod(2015, 12, 14, 2015, 12, 20) }, new List<DateOnly>());
			result.Should().Be.EqualTo(new DateOnly(2015, 12, 19));
		}

		private IEnumerable<IScheduleDayData> createOneWeek()
		{
			var mon = new ScheduleDayData(new DateOnly(2015, 12, 7));
			var tue = new ScheduleDayData(new DateOnly(2015, 12, 8));
			var wed = new ScheduleDayData(new DateOnly(2015, 12, 9));
			var thu = new ScheduleDayData(new DateOnly(2015, 12, 10));
			var fri = new ScheduleDayData(new DateOnly(2015, 12, 11));
			var sat = new ScheduleDayData(new DateOnly(2015, 12, 12));
			var sun = new ScheduleDayData(new DateOnly(2015, 12, 13)); //Lucia

			var dataCollection = new List<IScheduleDayData> { mon, tue, wed, thu, fri, sat, sun };

			return dataCollection;
		}

		private IEnumerable<IScheduleDayData> createTwoWeeks()
		{
			var mon1 = new ScheduleDayData(new DateOnly(2015, 12, 7));
			var tue1 = new ScheduleDayData(new DateOnly(2015, 12, 8));
			var wed1 = new ScheduleDayData(new DateOnly(2015, 12, 9));
			var thu1 = new ScheduleDayData(new DateOnly(2015, 12, 10));
			var fri1 = new ScheduleDayData(new DateOnly(2015, 12, 11));
			var sat1 = new ScheduleDayData(new DateOnly(2015, 12, 12));
			var sun1 = new ScheduleDayData(new DateOnly(2015, 12, 13)); //Lucia

			var mon2 = new ScheduleDayData(new DateOnly(2015, 12, 14));
			var tue2 = new ScheduleDayData(new DateOnly(2015, 12, 15));
			var wed2 = new ScheduleDayData(new DateOnly(2015, 12, 16));
			var thu2 = new ScheduleDayData(new DateOnly(2015, 12, 17));
			var fri2 = new ScheduleDayData(new DateOnly(2015, 12, 18));
			var sat2 = new ScheduleDayData(new DateOnly(2015, 12, 19));
			var sun2 = new ScheduleDayData(new DateOnly(2015, 12, 20));

			var dataCollection = new List<IScheduleDayData>
			{
				mon1,
				tue1,
				wed1,
				thu1,
				fri1,
				sat1,
				sun1,
				mon2,
				tue2,
				wed2,
				thu2,
				fri2,
				sat2,
				sun2
			};

			return dataCollection;
		}

		private class MatrixDataForTest : MatrixData
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
			public MatrixDataForTest(IEnumerable<IScheduleDayData> scheduleDayDataCollectionForTest)
				: base(null)
			{
				foreach (var scheduleDayData in scheduleDayDataCollectionForTest)
				{
					ScheduleDayDataDictionary.Add(scheduleDayData.DateOnly, scheduleDayData);
				}

			}

			public void SetDayOff(DateOnly date)
			{
				ScheduleDayDataDictionary[date].IsDayOff = true;
			}
		}
	}
}