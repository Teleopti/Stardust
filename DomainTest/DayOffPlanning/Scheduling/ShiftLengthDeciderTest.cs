using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning.Scheduling
{
	[TestFixture]
	public class ShiftLengthDeciderTest
	{
		private IShiftLengthDecider _target;
		private IDesiredShiftLengthCalculator _desiredShiftLengthCalculator;
		private IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
		private IScheduleMatrixPro _matrix;
		private SchedulingOptions _schedulingOptions;

		[SetUp]
		public void Setup()
		{
			_desiredShiftLengthCalculator = MockRepository.GenerateMock<IDesiredShiftLengthCalculator>();
			_target = new ShiftLengthDecider(_desiredShiftLengthCalculator);
			_workShiftMinMaxCalculator = MockRepository.GenerateMock<IWorkShiftMinMaxCalculator>();
			_matrix = MockRepository.GenerateMock<IScheduleMatrixPro>();
			_schedulingOptions = new SchedulingOptions();
		}

		[Test]
		public void ShouldNotFilterListIfNotUsingAverageShiftLengthAndNotUsingTeamBlockSameShift()
		{
			var workShift = new WorkShift(new ShiftCategory("test"));
			ShiftProjectionCache c1 = new ShiftProjectionCache(workShift, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
			ShiftProjectionCache c2 = new ShiftProjectionCache(workShift, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
			ShiftProjectionCache c3 = new ShiftProjectionCache(workShift, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));

			IList<ShiftProjectionCache> shiftList = new List<ShiftProjectionCache> { c1, c2, c3 };
			_schedulingOptions.UseAverageShiftLengths = false;

			var result = _target.FilterList(shiftList, _workShiftMinMaxCalculator, _matrix, _schedulingOptions, null, DateOnly.Today);
			Assert.AreEqual(3, result.Count);
		}

		[Test]
		public void ShouldNotFilterListIfNotWorkShiftLengthHintOptionAverageWorkTimeAndNotUsingTeamBlockSameShift()
		{
			var workShift = new WorkShift(new ShiftCategory("test"));
			ShiftProjectionCache c1 = new ShiftProjectionCache(workShift, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
			ShiftProjectionCache c2 = new ShiftProjectionCache(workShift, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
			ShiftProjectionCache c3 = new ShiftProjectionCache(workShift, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
			IList<ShiftProjectionCache> shiftList = new List<ShiftProjectionCache> { c1, c2, c3 };
			_schedulingOptions.UseAverageShiftLengths = false;

			var result = _target.FilterList(shiftList, _workShiftMinMaxCalculator, _matrix, _schedulingOptions, null, DateOnly.Today);
			Assert.AreEqual(3, result.Count);
		}

		[Test]
		public void ShouldReturnFilteredListAccordingToNearestShiftLength1()
		{
			var shiftCategory = new ShiftCategory("test");
			var activity = new Activity("Phone") {InContractTime = true};

			var workShift = new WorkShift(shiftCategory);
			workShift.LayerCollection.Add(new WorkShiftActivityLayer(activity,
				new DateTimePeriod(WorkShift.BaseDate.AddHours(8), WorkShift.BaseDate.AddHours(15.5))));

			var longWorkShift = new WorkShift(shiftCategory);
			longWorkShift.LayerCollection.Add(new WorkShiftActivityLayer(activity,
				new DateTimePeriod(WorkShift.BaseDate.AddHours(8), WorkShift.BaseDate.AddHours(15.75))));

			ShiftProjectionCache c1 = new ShiftProjectionCache(workShift, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
			ShiftProjectionCache c2 = new ShiftProjectionCache(longWorkShift, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
			ShiftProjectionCache c3 = new ShiftProjectionCache(workShift, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
			IList<ShiftProjectionCache> shiftList = new List<ShiftProjectionCache> {c1, c2, c3};
			_schedulingOptions.UseAverageShiftLengths = false;
			//should filter becouse we are using teamblock and same shift
			_schedulingOptions.UseBlock = true;
			_schedulingOptions.BlockSameShift = true;

			_desiredShiftLengthCalculator.Stub(x => x.FindAverageLength(_workShiftMinMaxCalculator, _matrix, _schedulingOptions, null))
				.Return(new TimeSpan(7, 36, 0));
			
			var result = _target.FilterList(shiftList, _workShiftMinMaxCalculator, _matrix, _schedulingOptions, null, DateOnly.Today);
			Assert.AreEqual(2, result.Count);
		}

		[Test]
		public void ShouldReturnFilteredListAccordingToNearestShiftLength2()
		{
			var shiftCategory = new ShiftCategory("test");
			var activity = new Activity("Phone") { InContractTime = true };

			var workShift = new WorkShift(shiftCategory);
			workShift.LayerCollection.Add(new WorkShiftActivityLayer(activity,
				new DateTimePeriod(WorkShift.BaseDate.AddHours(8), WorkShift.BaseDate.AddHours(15.5))));

			var longWorkShift = new WorkShift(shiftCategory);
			longWorkShift.LayerCollection.Add(new WorkShiftActivityLayer(activity,
				new DateTimePeriod(WorkShift.BaseDate.AddHours(8), WorkShift.BaseDate.AddHours(15.75))));

			ShiftProjectionCache c1 = new ShiftProjectionCache(workShift, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
			ShiftProjectionCache c2 = new ShiftProjectionCache(longWorkShift, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
			ShiftProjectionCache c3 = new ShiftProjectionCache(workShift, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
			IList<ShiftProjectionCache> shiftList = new List<ShiftProjectionCache> { c1, c2, c3 };

			_desiredShiftLengthCalculator.Stub(x => x.FindAverageLength(_workShiftMinMaxCalculator, _matrix, _schedulingOptions, null)).Return(new TimeSpan(8, 0, 0));
		
			var result = _target.FilterList(shiftList, _workShiftMinMaxCalculator, _matrix, _schedulingOptions, null, DateOnly.Today);
			
			Assert.AreEqual(1, result.Count);
			Assert.AreSame(c2, result[0]);
		}

		[Test]
		public void ShouldCheckParameters()
		{
			var result =_target.FilterList(null, _workShiftMinMaxCalculator, _matrix, _schedulingOptions, null, DateOnly.Today);
			Assert.IsNull(result);
		}
	}
}