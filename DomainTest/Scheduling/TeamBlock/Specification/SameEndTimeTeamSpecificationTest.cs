using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Specification
{
    public class SameEndTimeTeamSpecificationTest
    {
        private IBlockInfo _blockInfo;
        private IEditableShift _editableShift;
        private IScheduleMatrixPro _matrix1;
        private IScheduleMatrixPro _matrix2;
        private IList<IScheduleMatrixPro> _matrixList;
        private MockRepository _mock;
        private IProjectionService _projectionService;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IScheduleDay _scheduleDay3;
        private IScheduleDay _scheduleDay4;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private IScheduleRange _scheduleRange1;
        private IScheduleRange _scheduleRange2;
        private ISameEndTimeTeamSpecification _target;
        private ITeamBlockInfo _teamBlockInfo;
        private ITeamInfo _teamInfo;
        private DateOnly _today;
        private IVisualLayerCollection _visualLayerCollection;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
            _today = new DateOnly();
            _matrixList = new List<IScheduleMatrixPro>();
            _matrixList.Add(_scheduleMatrixPro);
            _teamInfo = _mock.StrictMock<ITeamInfo>();
            _blockInfo = _mock.StrictMock<IBlockInfo>();
            _teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
            _target = new SameEndTimeTeamSpecification();
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

        private void commonExpect(DateOnlyPeriod dateOnlyPeriod, IEnumerable<IScheduleMatrixPro> matrixList,
                                  DateTimePeriod dateTimePeriod)
        {
            Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
            Expect.Call(_blockInfo.BlockPeriod).Return(dateOnlyPeriod);
            Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
            Expect.Call(_teamInfo.MatrixesForGroupAndDate(_today)).Return(matrixList);
            Expect.Call(_scheduleDay1.GetEditorShift()).Return(_editableShift);
            Expect.Call(_editableShift.ProjectionService()).Return(_projectionService);
            Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
            Expect.Call(_visualLayerCollection.Period()).Return(dateTimePeriod);
        }

        private void scheduleDayExpectCalls(IScheduleRange scheduleRange, IScheduleDay scheduleDay,
                                            SchedulePartView partValue, DateOnly dateOnly,
                                            DateTimePeriod? dateTimePeriod)
        {
            Expect.Call(scheduleRange.ScheduledDay(dateOnly)).Return(scheduleDay);
            Expect.Call(scheduleDay.SignificantPart()).Return(partValue);
            Expect.Call(scheduleDay.GetEditorShift()).Return(_editableShift);
            Expect.Call(_editableShift.ProjectionService()).Return(_projectionService);
            Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
            Expect.Call(_visualLayerCollection.Period()).Return(dateTimePeriod);
        }

        [Test]
        public void ShouldReturnTrueIfNoMatrixFound()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(_today, _today);
            var dateTimePeriod = new DateTimePeriod(new DateTime(2013, 10, 31, 8, 0, 0, DateTimeKind.Utc),
                                                    new DateTime(2013, 10, 31, 15, 0, 0, DateTimeKind.Utc));
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> {};

            using (_mock.Record())
            {
                commonExpect(dateOnlyPeriod, matrixList, dateTimePeriod);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(_teamBlockInfo));
            }
        }

        [Test]
        public void ShouldReturnTrueIfSameEndTime()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(_today, _today);
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> {_matrix1};
            var dateTimePeriod = new DateTimePeriod(new DateTime(2013, 10, 31, 8, 0, 0, DateTimeKind.Utc),
                                                    new DateTime(2013, 10, 31, 15, 0, 0, DateTimeKind.Utc));

            using (_mock.Record())
            {
                commonExpect(dateOnlyPeriod, matrixList, dateTimePeriod);
                Expect.Call(_matrix1.ActiveScheduleRange).Return(_scheduleRange1);

                scheduleDayExpectCalls(_scheduleRange1, _scheduleDay1, SchedulePartView.MainShift, _today,
                                       dateTimePeriod);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(_teamBlockInfo));
            }
        }

        [Test]
        public void ShouldReturnFalseIfDifferentEndTime()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(_today, _today.AddDays(1));
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> {_matrix1};
            var dateTimePeriod1 = new DateTimePeriod(new DateTime(2013, 10, 31, 8, 0, 0, DateTimeKind.Utc),
                                                     new DateTime(2013, 10, 31, 15, 0, 0, DateTimeKind.Utc));
            var dateTimePeriod2 = new DateTimePeriod(new DateTime(2013, 10, 31, 8, 0, 0, DateTimeKind.Utc),
                                                     new DateTime(2013, 10, 31, 16, 0, 0, DateTimeKind.Utc));
            using (_mock.Record())
            {
                commonExpect(dateOnlyPeriod, matrixList, dateTimePeriod1);
                Expect.Call(_matrix1.ActiveScheduleRange).Return(_scheduleRange1);

                scheduleDayExpectCalls(_scheduleRange1, _scheduleDay1, SchedulePartView.MainShift, _today,
                                       dateTimePeriod1);
                scheduleDayExpectCalls(_scheduleRange1, _scheduleDay2, SchedulePartView.MainShift, _today.AddDays(1),
                                       dateTimePeriod2);
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(_teamBlockInfo));
            }
        }

        [Test]
        public void ShouldReturnFalseForTwoMatrixes()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(_today, _today.AddDays(1));
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> {_matrix1, _matrix2};
            var dateTimePeriod1 = new DateTimePeriod(new DateTime(2013, 10, 31, 8, 0, 0, DateTimeKind.Utc),
                                                     new DateTime(2013, 10, 31, 15, 0, 0, DateTimeKind.Utc));
            var dateTimePeriod2 = new DateTimePeriod(new DateTime(2013, 10, 31, 8, 0, 0, DateTimeKind.Utc),
                                                     new DateTime(2013, 10, 31, 16, 0, 0, DateTimeKind.Utc));
            using (_mock.Record())
            {
                commonExpect(dateOnlyPeriod, matrixList, dateTimePeriod1);
                Expect.Call(_matrix1.ActiveScheduleRange).Return(_scheduleRange1);

                scheduleDayExpectCalls(_scheduleRange1, _scheduleDay1, SchedulePartView.MainShift, _today,
                                       dateTimePeriod1);
                scheduleDayExpectCalls(_scheduleRange1, _scheduleDay2, SchedulePartView.MainShift, _today.AddDays(1),
                                       dateTimePeriod1);

                Expect.Call(_matrix2.ActiveScheduleRange).Return(_scheduleRange2);
                scheduleDayExpectCalls(_scheduleRange2, _scheduleDay3, SchedulePartView.MainShift, _today,
                                       dateTimePeriod1);
                scheduleDayExpectCalls(_scheduleRange2, _scheduleDay4, SchedulePartView.MainShift, _today.AddDays(1),
                                       dateTimePeriod2);
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(_teamBlockInfo));
            }
        }

        [Test]
        public void ShouldReturnTrueForTwoMatrixes()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(_today, _today.AddDays(1));
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> {_matrix1, _matrix2};
            var dateTimePeriod1 = new DateTimePeriod(new DateTime(2013, 10, 31, 8, 0, 0, DateTimeKind.Utc),
                                                     new DateTime(2013, 10, 31, 15, 0, 0, DateTimeKind.Utc));
            using (_mock.Record())
            {
                commonExpect(dateOnlyPeriod, matrixList, dateTimePeriod1);
                Expect.Call(_matrix1.ActiveScheduleRange).Return(_scheduleRange1);

                scheduleDayExpectCalls(_scheduleRange1, _scheduleDay1, SchedulePartView.MainShift, _today,
                                       dateTimePeriod1);
                scheduleDayExpectCalls(_scheduleRange1, _scheduleDay2, SchedulePartView.MainShift, _today.AddDays(1),
                                       dateTimePeriod1);

                Expect.Call(_matrix2.ActiveScheduleRange).Return(_scheduleRange2);
                scheduleDayExpectCalls(_scheduleRange2, _scheduleDay3, SchedulePartView.MainShift, _today,
                                       dateTimePeriod1);
                scheduleDayExpectCalls(_scheduleRange2, _scheduleDay4, SchedulePartView.MainShift, _today.AddDays(1),
                                       dateTimePeriod1);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(_teamBlockInfo));
            }
        }
    }
}