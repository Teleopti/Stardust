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
		private IScheduleMatrixPro _matrix;
		private List<IScheduleMatrixPro> _allMatrixes;
		private SchedulingOptions _scheduleOptions;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			_workShiftMinMaxCalculator = _mocks.StrictMock<IWorkShiftMinMaxCalculator>();
			_personalShiftMeetingTimeChecker = _mocks.StrictMock<IPersonalShiftMeetingTimeChecker>();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_allMatrixes = new List<IScheduleMatrixPro> { _matrix };
			_scheduleOptions = new SchedulingOptions();
			_target = new ContractTimeShiftFilter(_workShiftMinMaxCalculator);
		}

		[Test]
		public void ShouldFilterAccordingToContractTime()
		{
			IList<IShiftProjectionCache> shifts = new List<IShiftProjectionCache>();
			var dateOnly = new DateOnly(2013, 3, 1);
			var workShift1 = _mocks.StrictMock<IWorkShift>();
			var workShift2 = _mocks.StrictMock<IWorkShift>();
			var mainshift1 = _mocks.StrictMock<IMainShift>();
			var mainshift2 = _mocks.StrictMock<IMainShift>();
			var ps1 = _mocks.StrictMock<IProjectionService>();
			var ps2 = _mocks.StrictMock<IProjectionService>();
			var lc1 = _mocks.StrictMock<IVisualLayerCollection>();
			var lc2 = _mocks.StrictMock<IVisualLayerCollection>();

			var minMaxcontractTime = new MinMax<TimeSpan>(new TimeSpan(7, 0, 0), new TimeSpan(8, 0, 0));
			using (_mocks.Record())
			{
				ExpectCallForShouldFilterAccordingToContractTime(workShift1, mainshift1, workShift2, mainshift2, ps1, ps2, lc1, lc2, dateOnly, minMaxcontractTime);
			}

			IList<IShiftProjectionCache> retShifts;
			ShiftProjectionCache c1;
			ShiftProjectionCache c2;

			using (_mocks.Playback())
			{
				c1 = new ShiftProjectionCache(workShift1, _personalShiftMeetingTimeChecker);
				c1.SetDate(new DateOnly(2009, 1, 1), _timeZoneInfo);
				shifts.Add(c1);
				c2 = new ShiftProjectionCache(workShift2, _personalShiftMeetingTimeChecker);
				c2.SetDate(new DateOnly(2009, 1, 1), _timeZoneInfo);
				shifts.Add(c2);
				retShifts = _target.Filter(dateOnly, _allMatrixes, shifts, _scheduleOptions, new WorkShiftFinderResultForTest());
			}
			retShifts.Should().Contain(c1);
			retShifts.Count.Should().Be.EqualTo(1);
		}

	    private void ExpectCallForShouldFilterAccordingToContractTime(IWorkShift workShift1, IMainShift mainshift1,
	                                                                  IWorkShift workShift2, IMainShift mainshift2,
	                                                                  IProjectionService ps1, IProjectionService ps2,
	                                                                  IVisualLayerCollection lc1, IVisualLayerCollection lc2,
	                                                                  DateOnly dateOnly, MinMax<TimeSpan> minMaxcontractTime)
	    {
	        Expect.Call(workShift1.ToMainShift(new DateTime(2009, 1, 1), _timeZoneInfo)).Return(mainshift1);
	        Expect.Call(workShift2.ToMainShift(new DateTime(2009, 1, 1), _timeZoneInfo)).Return(mainshift2);
	        Expect.Call(workShift1.ProjectionService()).Return(ps1);
	        Expect.Call(workShift2.ProjectionService()).Return(ps2);
	        Expect.Call(ps1.CreateProjection()).Return(lc1);
	        Expect.Call(ps2.CreateProjection()).Return(lc2);

	        Expect.Call(lc1.ContractTime()).Return(new TimeSpan(7, 0, 0));
	        Expect.Call(lc2.ContractTime()).Return(new TimeSpan(10, 0, 0));
	        Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
	        Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(dateOnly, _matrix, _scheduleOptions))
	              .Return(minMaxcontractTime);
	    }
	}
}
