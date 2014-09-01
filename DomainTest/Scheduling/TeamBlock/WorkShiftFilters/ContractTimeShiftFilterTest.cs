using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.DomainTest.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class ContractTimeShiftFilterTest
	{
		private MockRepository _mocks;
		private IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
		private ContractTimeShiftFilter _target;
		private TimeZoneInfo _timeZoneInfo;
		private IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;
		private IScheduleMatrixPro _matrix1;
		private List<IScheduleMatrixPro> _allMatrixes;
		private SchedulingOptions _scheduleOptions;
		private IScheduleMatrixPro _matrix2;
		private DateOnly _dateOnly;
		private IWorkShift _workShift1;
		private IWorkShift _workShift2;
		private IEditableShift _mainshift1;
		private IEditableShift _mainshift2;
		private IProjectionService _ps1;
		private IProjectionService _ps2;
		private IVisualLayerCollection _lc1;
		private IVisualLayerCollection _lc2;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			_workShiftMinMaxCalculator = _mocks.StrictMock<IWorkShiftMinMaxCalculator>();
			_personalShiftMeetingTimeChecker = _mocks.StrictMock<IPersonalShiftMeetingTimeChecker>();
			_matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			_matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
			_allMatrixes = new List<IScheduleMatrixPro> {_matrix1, _matrix2};
			_scheduleOptions = new SchedulingOptions();
			_target = new ContractTimeShiftFilter(_workShiftMinMaxCalculator);
			_dateOnly = new DateOnly(2013, 3, 1);
			_workShift1 = _mocks.StrictMock<IWorkShift>();
			_workShift2 = _mocks.StrictMock<IWorkShift>();
			_mainshift1 = _mocks.StrictMock<IEditableShift>();
			_mainshift2 = _mocks.StrictMock<IEditableShift>();
			_ps1 = _mocks.StrictMock<IProjectionService>();
			_ps2 = _mocks.StrictMock<IProjectionService>();
			_lc1 = _mocks.StrictMock<IVisualLayerCollection>();
			_lc2 = _mocks.StrictMock<IVisualLayerCollection>();
		}

		[Test]
		public void ShouldFilterAccordingToContractTime()
		{
			IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();

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

			var c1 = new ShiftProjectionCache(_workShift1, _personalShiftMeetingTimeChecker);
			c1.SetDate(new DateOnly(2009, 1, 1), _timeZoneInfo);
			shifts.Add(c1);
			var c2 = new ShiftProjectionCache(_workShift2, _personalShiftMeetingTimeChecker);
			c2.SetDate(new DateOnly(2009, 1, 1), _timeZoneInfo);
			shifts.Add(c2);

			using (_mocks.Playback())
			{
				var retShifts = _target.Filter(_dateOnly, _allMatrixes, shifts, _scheduleOptions, new WorkShiftFinderResultForTest());

				retShifts.Should().Contain(c1);
				retShifts.Count.Should().Be.EqualTo(1);
			}
		}

		private void commonExpectCalls()
		{
			Expect.Call(_workShift1.ToEditorShift(new DateTime(2009, 1, 1), _timeZoneInfo)).Return(_mainshift1);
			Expect.Call(_workShift2.ToEditorShift(new DateTime(2009, 1, 1), _timeZoneInfo)).Return(_mainshift2);
			Expect.Call(_workShift1.ProjectionService()).Return(_ps1);
			Expect.Call(_workShift2.ProjectionService()).Return(_ps2);
			Expect.Call(_ps1.CreateProjection()).Return(_lc1);
			Expect.Call(_ps2.CreateProjection()).Return(_lc2);
			Expect.Call(_lc1.ContractTime()).Return(new TimeSpan(7, 0, 0));
			Expect.Call(_lc2.ContractTime()).Return(new TimeSpan(10, 0, 0));
		}

		[Test]
		public void ShouldFilterIfOneMinMaximumIsNull()
		{
			IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();

			var minMaxcontractTime1 = new MinMax<TimeSpan>(new TimeSpan(7, 0, 0), new TimeSpan(8, 0, 0));
			using (_mocks.Record())
			{
				commonExpectCalls();

				expectForShouldFilterIfOneMinMaximumIsNull(minMaxcontractTime1);
			}

			var c1 = new ShiftProjectionCache(_workShift1, _personalShiftMeetingTimeChecker);
			c1.SetDate(new DateOnly(2009, 1, 1), _timeZoneInfo);
			shifts.Add(c1);
			var c2 = new ShiftProjectionCache(_workShift2, _personalShiftMeetingTimeChecker);
			c2.SetDate(new DateOnly(2009, 1, 1), _timeZoneInfo);
			shifts.Add(c2);

			using (_mocks.Playback())
			{
				IList<IShiftProjectionCache> retShifts = _target.Filter(_dateOnly, _allMatrixes, shifts, _scheduleOptions,
					new WorkShiftFinderResultForTest());

				retShifts.Should().Contain(c1);
				retShifts.Count.Should().Be.EqualTo(1);
			}

		}

		[Test]
		public void ShouldFilterIfOneMinMaximumIsZero()
		{
			IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();

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

			var c1 = new ShiftProjectionCache(_workShift1, _personalShiftMeetingTimeChecker);
			c1.SetDate(new DateOnly(2009, 1, 1), _timeZoneInfo);
			shifts.Add(c1);
			var c2 = new ShiftProjectionCache(_workShift2, _personalShiftMeetingTimeChecker);
			c2.SetDate(new DateOnly(2009, 1, 1), _timeZoneInfo);
			shifts.Add(c2);

			using (_mocks.Playback())
			{
				IList<IShiftProjectionCache> retShifts = _target.Filter(_dateOnly, _allMatrixes, shifts, _scheduleOptions,
					new WorkShiftFinderResultForTest());

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
			IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();

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

			var c1 = new ShiftProjectionCache(_workShift1, _personalShiftMeetingTimeChecker);
			c1.SetDate(new DateOnly(2009, 1, 1), _timeZoneInfo);
			shifts.Add(c1);
			var c2 = new ShiftProjectionCache(_workShift2, _personalShiftMeetingTimeChecker);
			c2.SetDate(new DateOnly(2009, 1, 1), _timeZoneInfo);
			shifts.Add(c2);

			using (_mocks.Playback())
			{
				var retShifts = _target.Filter(_dateOnly, _allMatrixes, shifts, _scheduleOptions, new WorkShiftFinderResultForTest());

				retShifts.Should().Contain(c1);
				retShifts.Count.Should().Be.EqualTo(1);
			}
		}

		[Test]
		public void ShouldNotFilterIfNoMinMaxRestricted()
		{
			IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();

			using (_mocks.Record())
			{
				Expect.Call(_workShift1.ToEditorShift(new DateTime(2009, 1, 1), _timeZoneInfo)).Return(_mainshift1);
				Expect.Call(_workShift2.ToEditorShift(new DateTime(2009, 1, 1), _timeZoneInfo)).Return(_mainshift2);

				Expect.Call(() => _workShiftMinMaxCalculator.ResetCache()).Repeat.AtLeastOnce();
				Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(_dateOnly, _matrix1, _scheduleOptions))
					.Return(null);
				Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(_dateOnly, _matrix2, _scheduleOptions))
					.Return(null);
			}

			var c1 = new ShiftProjectionCache(_workShift1, _personalShiftMeetingTimeChecker);
			c1.SetDate(new DateOnly(2009, 1, 1), _timeZoneInfo);
			shifts.Add(c1);
			var c2 = new ShiftProjectionCache(_workShift2, _personalShiftMeetingTimeChecker);
			c2.SetDate(new DateOnly(2009, 1, 1), _timeZoneInfo);
			shifts.Add(c2);

			using (_mocks.Playback())
			{
				var retShifts = _target.Filter(_dateOnly, _allMatrixes, shifts, _scheduleOptions, new WorkShiftFinderResultForTest());

				retShifts.Count.Should().Be.EqualTo(0);
			}
		}


		[Test]
		public void ShouldCheckParameters()
		{
			var dateOnly = new DateOnly(2013, 3, 1);
			var result = _target.Filter(dateOnly, _allMatrixes, null, _scheduleOptions, new WorkShiftFinderResultForTest());
			Assert.IsNull(result);
			result = _target.Filter(dateOnly, _allMatrixes, new List<IShiftProjectionCache>(), _scheduleOptions, null);
			Assert.IsNull(result);
			result = _target.Filter(dateOnly, null, new List<IShiftProjectionCache>(), _scheduleOptions,
				new WorkShiftFinderResultForTest());
			Assert.IsNull(result);
			result = _target.Filter(dateOnly, _allMatrixes, new List<IShiftProjectionCache>(), _scheduleOptions,
				new WorkShiftFinderResultForTest());
			Assert.That(result.Count, Is.EqualTo(0));
		}
	}
}
