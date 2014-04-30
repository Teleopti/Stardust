using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.DayOffPlanning.Scheduling
{
	[TestFixture]
	public class ShiftLengthDeciderTest
	{
		private IShiftLengthDecider _target;
		private MockRepository _mocks;
		private IDesiredShiftLengthCalculator _desiredShiftLengthCalculator;
		private IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
		private IScheduleMatrixPro _matrix;
		private ISchedulingOptions _schedulingOptions;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_desiredShiftLengthCalculator = _mocks.StrictMock<IDesiredShiftLengthCalculator>();
			_target = new ShiftLengthDecider(_desiredShiftLengthCalculator);
			_workShiftMinMaxCalculator = _mocks.StrictMock<IWorkShiftMinMaxCalculator>();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_schedulingOptions = new SchedulingOptions();
		}

		[Test]
		public void ShouldNotFilterListIfNotUsingAverageShiftLengthAndNotUsingTeamBlockSameShift()
		{
			IShiftProjectionCache c1 = _mocks.StrictMock<IShiftProjectionCache>();
			IShiftProjectionCache c2 = _mocks.StrictMock<IShiftProjectionCache>();
			IShiftProjectionCache c3 = _mocks.StrictMock<IShiftProjectionCache>();
			IList<IShiftProjectionCache> shiftList = new List<IShiftProjectionCache> { c1, c2, c3 };
			_schedulingOptions.UseAverageShiftLengths = false;

			IList<IShiftProjectionCache> result;
			result = _target.FilterList(shiftList, _workShiftMinMaxCalculator, _matrix, _schedulingOptions);
			Assert.AreEqual(3, result.Count);
		}

		[Test]
		public void ShouldNotFilterListIfNotWorkShiftLengthHintOptionAverageWorkTimeAndNotUsingTeamBlockSameShift()
		{
			IShiftProjectionCache c1 = _mocks.StrictMock<IShiftProjectionCache>();
			IShiftProjectionCache c2 = _mocks.StrictMock<IShiftProjectionCache>();
			IShiftProjectionCache c3 = _mocks.StrictMock<IShiftProjectionCache>();
			IList<IShiftProjectionCache> shiftList = new List<IShiftProjectionCache> { c1, c2, c3 };
			_schedulingOptions.UseAverageShiftLengths = false;

			IList<IShiftProjectionCache> result;
			result = _target.FilterList(shiftList, _workShiftMinMaxCalculator, _matrix, _schedulingOptions);
			Assert.AreEqual(3, result.Count);
		}

		[Test]
		public void ShouldReturnFilteredListAccordingToNearestShiftLength1()
		{

			IShiftProjectionCache c1 = _mocks.StrictMock<IShiftProjectionCache>();
			IShiftProjectionCache c2 = _mocks.StrictMock<IShiftProjectionCache>();
			IShiftProjectionCache c3 = _mocks.StrictMock<IShiftProjectionCache>();
			IList<IShiftProjectionCache> shiftList = new List<IShiftProjectionCache> {c1, c2, c3};
			_schedulingOptions.UseAverageShiftLengths = false;
			//should filter becouse we are using teamblock and same shift
			_schedulingOptions.UseBlock = true;
			_schedulingOptions.BlockSameShift = true;

			using (_mocks.Record())
			{
				Expect.Call(_desiredShiftLengthCalculator.FindAverageLength(_workShiftMinMaxCalculator, _matrix, _schedulingOptions)).Return(
					new TimeSpan(7, 36, 0));
				Expect.Call(c1.WorkShiftProjectionContractTime).Return(TimeSpan.FromHours(7.5)).Repeat.AtLeastOnce();
				Expect.Call(c2.WorkShiftProjectionContractTime).Return(TimeSpan.FromHours(7.75)).Repeat.AtLeastOnce();
				Expect.Call(c3.WorkShiftProjectionContractTime).Return(TimeSpan.FromHours(7.5)).Repeat.AtLeastOnce();
			}

			IList<IShiftProjectionCache> result;

			using (_mocks.Playback())
			{
				result = _target.FilterList(shiftList, _workShiftMinMaxCalculator, _matrix, _schedulingOptions);
			}
			
			Assert.AreEqual(2, result.Count);
		}

		[Test]
		public void ShouldReturnFilteredListAccordingToNearestShiftLength2()
		{

			IShiftProjectionCache c1 = _mocks.StrictMock<IShiftProjectionCache>();
			IShiftProjectionCache c2 = _mocks.StrictMock<IShiftProjectionCache>();
			IShiftProjectionCache c3 = _mocks.StrictMock<IShiftProjectionCache>();
			IList<IShiftProjectionCache> shiftList = new List<IShiftProjectionCache> { c1, c2, c3 };

			using (_mocks.Record())
			{
				Expect.Call(_desiredShiftLengthCalculator.FindAverageLength(_workShiftMinMaxCalculator, _matrix, _schedulingOptions)).Return(
					new TimeSpan(8, 0, 0));
				Expect.Call(c1.WorkShiftProjectionContractTime).Return(TimeSpan.FromHours(7.5)).Repeat.AtLeastOnce();
				Expect.Call(c2.WorkShiftProjectionContractTime).Return(TimeSpan.FromHours(7.75)).Repeat.AtLeastOnce();
				Expect.Call(c3.WorkShiftProjectionContractTime).Return(TimeSpan.FromHours(7.5)).Repeat.AtLeastOnce();
			}

			IList<IShiftProjectionCache> result;

			using (_mocks.Playback())
			{
				result = _target.FilterList(shiftList, _workShiftMinMaxCalculator, _matrix, _schedulingOptions);
			}

			Assert.AreEqual(1, result.Count);
			Assert.AreSame(c2, result[0]);
		}

		[Test]
		public void ShouldCheckParameters()
		{
			var result =_target.FilterList(null, _workShiftMinMaxCalculator, _matrix, _schedulingOptions);
			Assert.IsNull(result);
		}
	}
}