using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	public class ContractTimeShiftFilterTest
	{
		private MockRepository _mocks;
		private IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
		private ContractTimeShiftFilter _target;
		private TimeZoneInfo _timeZoneInfo;
		private IScheduleMatrixPro _matrix1;
		private List<IScheduleMatrixPro> _allMatrixes;
		private SchedulingOptions _scheduleOptions;
		private IScheduleMatrixPro _matrix2;
		private DateOnly _dateOnly;
		private IWorkShift _workShift1;
		private IWorkShift _workShift2;
		private IVisualLayerCollection _lc1;
		private IVisualLayerCollection _lc2;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_timeZoneInfo = TimeZoneInfo.Utc;
			_workShiftMinMaxCalculator = _mocks.StrictMock<IWorkShiftMinMaxCalculator>();
			_matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			_matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
			_allMatrixes = new List<IScheduleMatrixPro> {_matrix1, _matrix2};
			_scheduleOptions = new SchedulingOptions();
			_target = new ContractTimeShiftFilter(()=>_workShiftMinMaxCalculator);
			_dateOnly = new DateOnly(2013, 3, 1);
			_workShift1 = _mocks.StrictMock<IWorkShift>();
			_workShift2 = _mocks.StrictMock<IWorkShift>();
			_lc1 = _mocks.StrictMock<IVisualLayerCollection>();
			_lc2 = _mocks.StrictMock<IVisualLayerCollection>();
		}

		[Test]
		public void ShouldFilterAccordingToContractTime()
		{
			IList<ShiftProjectionCache> shifts = new List<ShiftProjectionCache>();

			var minMaxcontractTime = new MinMax<TimeSpan>(new TimeSpan(7, 0, 0), new TimeSpan(8, 0, 0));
			using (_mocks.Record())
			{
				commonExpectCalls();

				Expect.Call(() => _workShiftMinMaxCalculator.ResetCache()).Repeat.AtLeastOnce();
				Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(_dateOnly, _matrix1, _scheduleOptions))
					.Return(minMaxcontractTime);
				Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(_dateOnly, _matrix2, _scheduleOptions))
					.Return(minMaxcontractTime);
			}

			var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 1, 1), _timeZoneInfo);
			var c1 = new ShiftProjectionCache(_workShift1, dateOnlyAsDateTimePeriod);
			shifts.Add(c1);
			var c2 = new ShiftProjectionCache(_workShift2, dateOnlyAsDateTimePeriod);
			shifts.Add(c2);

			using (_mocks.Playback())
			{
				var retShifts = _target.Filter(_dateOnly, _allMatrixes, shifts, _scheduleOptions);

				retShifts.Should().Contain(c1);
				retShifts.Count.Should().Be.EqualTo(1);
			}
		}

		private void commonExpectCalls()
		{
			Expect.Call(_workShift1.Projection).Return(_lc1);
			Expect.Call(_workShift2.Projection).Return(_lc2);
			Expect.Call(_lc1.ContractTime()).Return(new TimeSpan(7, 0, 0));
			Expect.Call(_lc2.ContractTime()).Return(new TimeSpan(10, 0, 0));
		}

		[Test]
		public void ShouldFilterIfOneMinMaximumIsNull()
		{
			IList<ShiftProjectionCache> shifts = new List<ShiftProjectionCache>();

			var minMaxcontractTime1 = new MinMax<TimeSpan>(new TimeSpan(7, 0, 0), new TimeSpan(8, 0, 0));
			using (_mocks.Record())
			{
				commonExpectCalls();

				expectForShouldFilterIfOneMinMaximumIsNull(minMaxcontractTime1);
			}

			var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 1, 1), _timeZoneInfo);
			var c1 = new ShiftProjectionCache(_workShift1, dateOnlyAsDateTimePeriod);
			shifts.Add(c1);
			var c2 = new ShiftProjectionCache(_workShift2, dateOnlyAsDateTimePeriod);
			shifts.Add(c2);

			using (_mocks.Playback())
			{
				IList<ShiftProjectionCache> retShifts = _target.Filter(_dateOnly, _allMatrixes, shifts, _scheduleOptions);

				retShifts.Should().Contain(c1);
				retShifts.Count.Should().Be.EqualTo(1);
			}

		}

		[Test]
		public void ShouldFilterIfOneMinMaximumIsZero()
		{
			IList<ShiftProjectionCache> shifts = new List<ShiftProjectionCache>();

			var minMaxcontractTime1 = new MinMax<TimeSpan>(new TimeSpan(7, 0, 0), new TimeSpan(8, 0, 0));
			var minMaxZero = new MinMax<TimeSpan>(new TimeSpan(0,0,0 ), new TimeSpan(0, 0, 0));
			using (_mocks.Record())
			{
				commonExpectCalls();

				Expect.Call(() => _workShiftMinMaxCalculator.ResetCache()).Repeat.AtLeastOnce();
				Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(_dateOnly, _matrix1, _scheduleOptions))
					.Return(minMaxcontractTime1);
				Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(_dateOnly, _matrix2, _scheduleOptions))
					.Return(minMaxZero);
			}

			var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 1, 1), _timeZoneInfo);
			var c1 = new ShiftProjectionCache(_workShift1, dateOnlyAsDateTimePeriod);
			shifts.Add(c1);
			var c2 = new ShiftProjectionCache(_workShift2, dateOnlyAsDateTimePeriod);
			shifts.Add(c2);

			using (_mocks.Playback())
			{
				IList<ShiftProjectionCache> retShifts = _target.Filter(_dateOnly, _allMatrixes, shifts, _scheduleOptions);

				retShifts.Should().Contain(c1);
				retShifts.Count.Should().Be.EqualTo(1);
			}

		}

		private void expectForShouldFilterIfOneMinMaximumIsNull(MinMax<TimeSpan> minMaxcontractTime1)
		{
			Expect.Call(() => _workShiftMinMaxCalculator.ResetCache()).Repeat.AtLeastOnce();
			Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(_dateOnly, _matrix1, _scheduleOptions))
				.Return(minMaxcontractTime1);
			Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(_dateOnly, _matrix2, _scheduleOptions))
				.Return(null);
		}

		[Test]
		public void ShouldMergeMinMax()
		{
			IList<ShiftProjectionCache> shifts = new List<ShiftProjectionCache>();

			var minMaxcontractTime1 = new MinMax<TimeSpan>(new TimeSpan(7, 0, 0), new TimeSpan(8, 0, 0));
			var minMaxcontractTime2 = new MinMax<TimeSpan>(new TimeSpan(6, 0, 0), new TimeSpan(7, 30, 0));
			using (_mocks.Record())
			{
				commonExpectCalls();

				Expect.Call(() => _workShiftMinMaxCalculator.ResetCache()).Repeat.AtLeastOnce();
				Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(_dateOnly, _matrix1, _scheduleOptions))
					.Return(minMaxcontractTime1);
				Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(_dateOnly, _matrix2, _scheduleOptions))
					.Return(minMaxcontractTime2);
			}

			var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 1, 1), _timeZoneInfo);
			var c1 = new ShiftProjectionCache(_workShift1, dateOnlyAsDateTimePeriod);
			shifts.Add(c1);
			var c2 = new ShiftProjectionCache(_workShift2, dateOnlyAsDateTimePeriod);
			shifts.Add(c2);

			using (_mocks.Playback())
			{
				var retShifts = _target.Filter(_dateOnly, _allMatrixes, shifts, _scheduleOptions);

				retShifts.Should().Contain(c1);
				retShifts.Count.Should().Be.EqualTo(1);
			}
		}

		[Test]
		public void ShouldNotFilterIfNoMinMaxRestricted()
		{
			IList<ShiftProjectionCache> shifts = new List<ShiftProjectionCache>();

			using (_mocks.Record())
			{
				Expect.Call(() => _workShiftMinMaxCalculator.ResetCache()).Repeat.AtLeastOnce();
				Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(_dateOnly, _matrix1, _scheduleOptions))
					.Return(null);
				Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(_dateOnly, _matrix2, _scheduleOptions))
					.Return(null);
			}

			var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 1, 1), _timeZoneInfo);
			var c1 = new ShiftProjectionCache(_workShift1, dateOnlyAsDateTimePeriod);
			shifts.Add(c1);
			var c2 = new ShiftProjectionCache(_workShift2, dateOnlyAsDateTimePeriod);
			shifts.Add(c2);

			using (_mocks.Playback())
			{
				var retShifts = _target.Filter(_dateOnly, _allMatrixes, shifts, _scheduleOptions);

				retShifts.Count.Should().Be.EqualTo(0);
			}
		}


		[Test]
		public void ShouldCheckParameters()
		{
			var dateOnly = new DateOnly(2013, 3, 1);
			var result = _target.Filter(dateOnly, _allMatrixes, null, _scheduleOptions);
			Assert.IsNull(result);
			result = _target.Filter(dateOnly, null, new List<ShiftProjectionCache>(), _scheduleOptions);
			Assert.IsNull(result);
			result = _target.Filter(dateOnly, _allMatrixes, new List<ShiftProjectionCache>(), _scheduleOptions);
			Assert.That(result.Count, Is.EqualTo(0));
		}
	}
}
