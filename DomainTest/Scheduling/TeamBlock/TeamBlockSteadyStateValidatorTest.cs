using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class TeamBlockSteadyStateValidatorTest
    {
        private MockRepository _mock;
        private ITeamBlockSteadyStateValidator _target;
        private ITeamInfo _teamInfo;
        private IBlockInfo _blockInfo;
        private IList<IScheduleMatrixPro> _matrixList;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private DateOnly _today;
        private IScheduleDay _todayScheduleDay;
        private IScheduleDay _tomorrowScheduleDay;
        private DateOnly _tomorrow;
        private DateOnly _dayAfterTomorrow;
	    private IScheduleDayEquator _scheduleDayEquator;
        private IScheduleDay _dayAfterTomorrowDay;
		private ITeamBlockInfo _teamBlockInfo;
		private ISchedulingOptions _schedulingOptions;
	    private IScheduleRange _range;
	    private DateTimePeriod _sampleShiftPeriod;
		private IEditableShift _editableShift;
		private IProjectionService _projectionService;
		private IVisualLayerCollection _visualLayerCollection;
		
        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
	        _scheduleDayEquator = _mock.StrictMock<IScheduleDayEquator>();
			_target = new TeamBlockSteadyStateValidator(_scheduleDayEquator);
            _scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
            _todayScheduleDay = _mock.StrictMock<IScheduleDay>();
            _tomorrowScheduleDay = _mock.StrictMock<IScheduleDay>();
            _dayAfterTomorrowDay = _mock.StrictMock<IScheduleDay>();
            _today = new DateOnly();
            _tomorrow = _today.AddDays(1);
            _dayAfterTomorrow = _tomorrow.AddDays(1);
			_matrixList = new List<IScheduleMatrixPro>();
			_matrixList.Add(_scheduleMatrixPro);
	        _teamInfo = _mock.StrictMock<ITeamInfo>();
			_blockInfo = new BlockInfo(new DateOnlyPeriod(_today, _today.AddDays(2)));
			_teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
			_schedulingOptions = new SchedulingOptions();
	        _range = _mock.StrictMock<IScheduleRange>();
			_sampleShiftPeriod = new DateTimePeriod(2000, 1, 1, 2000, 1, 1);
			_editableShift = _mock.StrictMock<IEditableShift>();
			_projectionService = _mock.StrictMock<IProjectionService>();
			_visualLayerCollection = _mock.StrictMock<IVisualLayerCollection>();
        }

		[Test]
		public void ShouldReturnTrueForNonscheduledDays()
		{

			using (_mock.Record())
			{
				commonMocksForSampleDay(false);
			}

			using (_mock.Playback())
			{
				Assert.IsTrue(_target.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
			}
		}

		[Test]
		public void ShouldReturnTrueIfNoParametersInSchedulingOptions()
		{

			using (_mock.Record())
			{
				commonMocksForSampleDay(true);
			}

			using (_mock.Playback())
			{
				Assert.IsTrue(_target.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
			}
		}

        [Test]
        public void ShouldReturnFalseIfAllAreNotSameStartTimeForSameStartTime()
        {
            _schedulingOptions.UseTeamBlockSameStartTime = true;

            using (_mock.Record())
            {
	            commonMocksForSampleDay(true);
				Expect.Call(_scheduleMatrixPro.ActiveScheduleRange).Return(_range);
				Expect.Call(_range.ScheduledDay(_today)).Return(_todayScheduleDay);
				Expect.Call(_todayScheduleDay.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call(_range.ScheduledDay(_tomorrow)).Return(_tomorrowScheduleDay);
				Expect.Call(_tomorrowScheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				commonMocksForMainShiftPeriod(_sampleShiftPeriod.MovePeriod(TimeSpan.FromMinutes(1)));
            }
			using (_mock.Playback())
			{
				Assert.IsFalse(_target.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
			}
        }

		[Test]
		public void ShouldReturnTrueIfAllAreSameStartTimeForSameStartTime()
		{
			_schedulingOptions.UseTeamBlockSameStartTime = true;

			using (_mock.Record())
			{
				commonMocksForSampleDay(true);
				Expect.Call(_scheduleMatrixPro.ActiveScheduleRange).Return(_range);
				Expect.Call(_range.ScheduledDay(_today)).Return(_todayScheduleDay);
				Expect.Call(_todayScheduleDay.SignificantPart()).Return(SchedulePartView.None);
				commonMocksForMainShiftPeriod(_sampleShiftPeriod);
				Expect.Call(_range.ScheduledDay(_tomorrow)).Return(_tomorrowScheduleDay);
				Expect.Call(_tomorrowScheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_range.ScheduledDay(_dayAfterTomorrow)).Return(_dayAfterTomorrowDay);
				Expect.Call(_dayAfterTomorrowDay.SignificantPart()).Return(SchedulePartView.None);
			}
			using (_mock.Playback())
			{
				Assert.IsTrue(_target.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
			}
		}

		[Test]
		public void ShouldReturnFalseIfAllAreNotSameEndTimeForSameEndTime()
		{
			_schedulingOptions.UseTeamBlockSameEndTime = true;

			using (_mock.Record())
			{
				commonMocksForSampleDay(true);
				Expect.Call(_scheduleMatrixPro.ActiveScheduleRange).Return(_range);
				Expect.Call(_range.ScheduledDay(_today)).Return(_todayScheduleDay);
				Expect.Call(_todayScheduleDay.SignificantPart()).Return(SchedulePartView.None);
				commonMocksForMainShiftPeriod(_sampleShiftPeriod.MovePeriod(TimeSpan.FromMinutes(1)));
				Expect.Call(_range.ScheduledDay(_tomorrow)).Return(_tomorrowScheduleDay);
				Expect.Call(_tomorrowScheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
			}
			using (_mock.Playback())
			{
				Assert.False(_target.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
			}
		}

		[Test]
		public void ShouldReturnTrueIfAllAreSameEndTimeForSameEndTime()
		{
			_schedulingOptions.UseTeamBlockSameEndTime = true;

			using (_mock.Record())
			{
				commonMocksForSampleDay(true);
				Expect.Call(_scheduleMatrixPro.ActiveScheduleRange).Return(_range);
				Expect.Call(_range.ScheduledDay(_today)).Return(_todayScheduleDay);
				Expect.Call(_todayScheduleDay.SignificantPart()).Return(SchedulePartView.None);
				commonMocksForMainShiftPeriod(_sampleShiftPeriod);
				Expect.Call(_range.ScheduledDay(_tomorrow)).Return(_tomorrowScheduleDay);
				Expect.Call(_tomorrowScheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_range.ScheduledDay(_dayAfterTomorrow)).Return(_dayAfterTomorrowDay);
				Expect.Call(_dayAfterTomorrowDay.SignificantPart()).Return(SchedulePartView.None);
			}
			using (_mock.Playback())
			{
				Assert.IsTrue(_target.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
			}
		}

		[Test]
		public void ShouldReturnFalseIfAllAreNotSameForShiftCategory()
		{
			_schedulingOptions.UseTeamBlockSameShiftCategory = true;
			var sampleAssignment = PersonAssignmentFactory.CreateAssignmentWithThreeMainshiftLayers();
			var foundAssignment = PersonAssignmentFactory.CreateAssignmentWithThreeMainshiftLayers();

			using (_mock.Record())
			{
				commonMocksForSampleDay(true);
				Expect.Call(_tomorrowScheduleDay.PersonAssignment()).Return(sampleAssignment);
				Expect.Call(_scheduleMatrixPro.ActiveScheduleRange).Return(_range);
				Expect.Call(_range.ScheduledDay(_today)).Return(_todayScheduleDay);
				Expect.Call(_todayScheduleDay.SignificantPart()).Return(SchedulePartView.None);
				
				Expect.Call(_range.ScheduledDay(_tomorrow)).Return(_tomorrowScheduleDay);
				Expect.Call(_tomorrowScheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_tomorrowScheduleDay.PersonAssignment()).Return(foundAssignment);
			}
			using (_mock.Playback())
			{
				Assert.False(_target.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
			}
		}

		[Test]
		public void ShouldReturnTrueIfAllAreSameForShiftCategory()
		{
			_schedulingOptions.UseTeamBlockSameShiftCategory = true;
			var sampleAssignment = PersonAssignmentFactory.CreateAssignmentWithThreeMainshiftLayers();
			var foundAssignment = sampleAssignment;

			using (_mock.Record())
			{
				commonMocksForSampleDay(true);
				Expect.Call(_tomorrowScheduleDay.PersonAssignment()).Return(sampleAssignment);
				Expect.Call(_scheduleMatrixPro.ActiveScheduleRange).Return(_range);
				Expect.Call(_range.ScheduledDay(_today)).Return(_todayScheduleDay);
				Expect.Call(_todayScheduleDay.SignificantPart()).Return(SchedulePartView.None);

				Expect.Call(_range.ScheduledDay(_tomorrow)).Return(_tomorrowScheduleDay);
				Expect.Call(_tomorrowScheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_tomorrowScheduleDay.PersonAssignment()).Return(foundAssignment);
				Expect.Call(_range.ScheduledDay(_dayAfterTomorrow)).Return(_dayAfterTomorrowDay);
				Expect.Call(_dayAfterTomorrowDay.SignificantPart()).Return(SchedulePartView.None);
			}
			using (_mock.Playback())
			{
				Assert.IsTrue(_target.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
			}
		}

		[Test]
		public void ShouldReturnFalseIfAllAreNotSameForSameShift()
		{
			_schedulingOptions.UseTeamBlockSameShift = true;

			var shift = EditableShiftFactory.CreateEditorShiftWithThreeActivityLayers();

			using (_mock.Record())
			{
				commonMocksForSampleDay(true);
				Expect.Call(_scheduleMatrixPro.ActiveScheduleRange).Return(_range);
				Expect.Call(_range.ScheduledDay(_today)).Return(_todayScheduleDay);
				Expect.Call(_todayScheduleDay.SignificantPart()).Return(SchedulePartView.None);

				Expect.Call(_range.ScheduledDay(_tomorrow)).Return(_tomorrowScheduleDay);
				Expect.Call(_tomorrowScheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_tomorrowScheduleDay.GetEditorShift())
				      .Return(shift);
				Expect.Call(_scheduleDayEquator.MainShiftEquals(shift, shift)).IgnoreArguments().Return(false);
			}
			using (_mock.Playback())
			{
				Assert.False(_target.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
			}
		}

		[Test]
		public void ShouldReturnTrueIfAllAreSameForSameShift()
		{
			_schedulingOptions.UseTeamBlockSameShift = true;

			var shift = EditableShiftFactory.CreateEditorShiftWithThreeActivityLayers();

			using (_mock.Record())
			{
				commonMocksForSampleDay(true);
				Expect.Call(_scheduleMatrixPro.ActiveScheduleRange).Return(_range);
				Expect.Call(_range.ScheduledDay(_today)).Return(_todayScheduleDay);
				Expect.Call(_todayScheduleDay.SignificantPart()).Return(SchedulePartView.None);

				Expect.Call(_range.ScheduledDay(_tomorrow)).Return(_tomorrowScheduleDay);
				Expect.Call(_tomorrowScheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_tomorrowScheduleDay.GetEditorShift())
					  .Return(shift);
				Expect.Call(_scheduleDayEquator.MainShiftEquals(shift, shift)).IgnoreArguments().Return(true);
				Expect.Call(_range.ScheduledDay(_dayAfterTomorrow)).Return(_dayAfterTomorrowDay);
				Expect.Call(_dayAfterTomorrowDay.SignificantPart()).Return(SchedulePartView.None);
			}
			using (_mock.Playback())
			{
				Assert.IsTrue(_target.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
			}
		}

		private void commonMocksForSampleDay(bool findDay)
		{
			Expect.Call(_teamInfo.MatrixesForGroupAndDate(_today)).Return(_matrixList);
			Expect.Call(_scheduleMatrixPro.ActiveScheduleRange).Return(_range);
			Expect.Call(_range.ScheduledDay(_today)).Return(_todayScheduleDay);
			Expect.Call(_range.ScheduledDay(_tomorrow)).Return(_tomorrowScheduleDay);
			if (!findDay)
			{
				Expect.Call(_range.ScheduledDay(_dayAfterTomorrow)).Return(_dayAfterTomorrowDay);
				Expect.Call(_todayScheduleDay.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call(_tomorrowScheduleDay.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call(_dayAfterTomorrowDay.SignificantPart()).Return(SchedulePartView.None);
			}
			else
			{
				Expect.Call(_todayScheduleDay.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call(_tomorrowScheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				commonMocksForMainShiftPeriod(_sampleShiftPeriod);
			}
		}

		private void commonMocksForMainShiftPeriod(DateTimePeriod period)
		{
			Expect.Call(_tomorrowScheduleDay.GetEditorShift()).Return(_editableShift);
			Expect.Call(_editableShift.ProjectionService()).Return(_projectionService);
			Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
			Expect.Call(_visualLayerCollection.Period()).Return(period);
		}
    }
}
