using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Specification
{
    public class SameStartTimeTeamSpecificationTest
    {
        private IBlockInfo _blockInfo;
        private IEditableShift _editableShift;
        private IScheduleMatrixPro _matrix1;
        private IScheduleMatrixPro _matrix2;
        private MockRepository _mock;
        private IProjectionService _projectionService;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IScheduleDay _scheduleDay3;
        private IScheduleDay _scheduleDay4;
        private IScheduleRange _scheduleRange1;
        private IScheduleRange _scheduleRange2;
        private ISameStartTimeTeamSpecification _target;
        private ITeamBlockInfo _teamBlockInfo;
        private ITeamInfo _teamInfo;
        private DateOnly _today;
        private IVisualLayerCollection _visualLayerCollection;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _today = new DateOnly();
            _teamInfo = _mock.StrictMock<ITeamInfo>();
            _blockInfo = _mock.StrictMock<IBlockInfo>();
            _teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
			_target = new SameStartTimeTeamSpecification();
            _scheduleDay1 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay2 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay3 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay4 = _mock.StrictMock<IScheduleDay>();
            _matrix1 = _mock.StrictMock<IScheduleMatrixPro>();
            _matrix2 = _mock.StrictMock<IScheduleMatrixPro>();
            _scheduleRange1 = _mock.StrictMock<IScheduleRange>();
            _scheduleRange2 = _mock.StrictMock<IScheduleRange>();
            _editableShift = _mock.StrictMock<IEditableShift>();
            _projectionService = _mock.StrictMock<IProjectionService>();
            _visualLayerCollection = _mock.StrictMock<IVisualLayerCollection>();
        }

        [Test]
        public void ShouldReturnTrueIfNoMatrixFound()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(_today, _today);
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { };

            using (_mock.Record())
            {
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_blockInfo.BlockPeriod).Return(dateOnlyPeriod);
	            Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
	            Expect.Call(_teamInfo.MatrixesForGroup()).Return(matrixList);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(_teamBlockInfo));
            }
        }

        [Test]
        public void ShouldReturnFalseForTeamHasDifferentStartTime()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(_today, _today);
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _matrix1, _matrix2 };
            var dateTimePeriod1 = new DateTimePeriod(new DateTime(2013, 10, 31, 7, 0, 0, DateTimeKind.Utc),
                                                     new DateTime(2013, 10, 31, 15, 0, 0, DateTimeKind.Utc));
			var dateTimePeriod2 = new DateTimePeriod(new DateTime(2013, 10, 31, 9, 0, 0, DateTimeKind.Utc),
                                                     new DateTime(2013, 10, 31, 16, 0, 0, DateTimeKind.Utc));
            using (_mock.Record())
            {
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_blockInfo.BlockPeriod).Return(dateOnlyPeriod);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.MatrixesForGroup()).Return(matrixList);
				Expect.Call(_matrix1.ActiveScheduleRange).Return(_scheduleRange1);
				Expect.Call(_matrix2.ActiveScheduleRange).Return(_scheduleRange2);
				Expect.Call(_scheduleRange1.ScheduledDay(_today)).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay1.GetEditorShift()).Return(_editableShift);
				Expect.Call(_editableShift.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(dateTimePeriod1);
				Expect.Call(_scheduleRange2.ScheduledDay(_today)).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.GetEditorShift()).Return(_editableShift);
				Expect.Call(_editableShift.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(dateTimePeriod2);
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(_teamBlockInfo));
            }
        }

        [Test]
        public void ShouldReturnTrueForTeamHasStartTime()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(_today, _today);
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _matrix1, _matrix2 };
            var dateTimePeriod1 = new DateTimePeriod(new DateTime(2013, 10, 31, 7, 0, 0, DateTimeKind.Utc),
                                                     new DateTime(2013, 10, 31, 15, 0, 0, DateTimeKind.Utc));
            using (_mock.Record())
            {
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_blockInfo.BlockPeriod).Return(dateOnlyPeriod);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.MatrixesForGroup()).Return(matrixList);
				Expect.Call(_matrix1.ActiveScheduleRange).Return(_scheduleRange1);
				Expect.Call(_matrix2.ActiveScheduleRange).Return(_scheduleRange2);
				Expect.Call(_scheduleRange1.ScheduledDay(_today)).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay1.GetEditorShift()).Return(_editableShift);
				Expect.Call(_editableShift.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(dateTimePeriod1);
				Expect.Call(_scheduleRange2.ScheduledDay(_today)).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.GetEditorShift()).Return(_editableShift);
				Expect.Call(_editableShift.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(dateTimePeriod1);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(_teamBlockInfo));
            }
        }

        [Test]
        public void ShouldReturnTrueForTwoDays()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(_today, _today.AddDays(1));
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _matrix1, _matrix2 };
            var dateTimePeriod1 = new DateTimePeriod(new DateTime(2013, 10, 31, 7, 0, 0, DateTimeKind.Utc),
                                                     new DateTime(2013, 10, 31, 15, 0, 0, DateTimeKind.Utc));
	        var dateTimePeriod2 = new DateTimePeriod(new DateTime(2013, 10, 31, 7, 0, 0, DateTimeKind.Utc),
	                                                 new DateTime(2013, 10, 31, 16, 0, 0, DateTimeKind.Utc));
			var dateTimePeriod3 = new DateTimePeriod(new DateTime(2013, 10, 31, 7, 0, 0, DateTimeKind.Utc),
                                                     new DateTime(2013, 10, 31, 10, 0, 0, DateTimeKind.Utc));
			var dateTimePeriod4 = new DateTimePeriod(new DateTime(2013, 10, 31, 7, 0, 0, DateTimeKind.Utc),
                                                     new DateTime(2013, 10, 31, 9, 0, 0, DateTimeKind.Utc));
            using (_mock.Record())
            {
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_blockInfo.BlockPeriod).Return(dateOnlyPeriod);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.MatrixesForGroup()).Return(matrixList);
				Expect.Call(_matrix1.ActiveScheduleRange).Return(_scheduleRange1).Repeat.Twice();
				Expect.Call(_matrix2.ActiveScheduleRange).Return(_scheduleRange2).Repeat.Twice();
				
				Expect.Call(_scheduleRange1.ScheduledDay(_today)).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay1.GetEditorShift()).Return(_editableShift);
				Expect.Call(_editableShift.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(dateTimePeriod1);
				Expect.Call(_scheduleRange1.ScheduledDay(_today.AddDays(1))).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.GetEditorShift()).Return(_editableShift);
				Expect.Call(_editableShift.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(dateTimePeriod2);
				
				Expect.Call(_scheduleRange2.ScheduledDay(_today)).Return(_scheduleDay3);
				Expect.Call(_scheduleDay3.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay3.GetEditorShift()).Return(_editableShift);
				Expect.Call(_editableShift.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(dateTimePeriod3);
				Expect.Call(_scheduleRange2.ScheduledDay(_today.AddDays(1))).Return(_scheduleDay4);
				Expect.Call(_scheduleDay4.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay4.GetEditorShift()).Return(_editableShift);
				Expect.Call(_editableShift.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(dateTimePeriod4);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(_teamBlockInfo));
            }
        }
    }
}