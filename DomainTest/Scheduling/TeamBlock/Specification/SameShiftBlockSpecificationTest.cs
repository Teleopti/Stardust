using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Specification
{
    public class SameShiftBlockSpecificationTest
    {
        private IBlockInfo _blockInfo;
        private IEditableShift _editableShift;
        private IScheduleMatrixPro _matrix1;
        private IScheduleMatrixPro _matrix2;
        private IList<IScheduleMatrixPro> _matrixList;
        private MockRepository _mock;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IScheduleDay _scheduleDay3;
        private IScheduleDay _scheduleDay4;
        private IScheduleDayEquator _scheduleDayEquator;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private IScheduleRange _scheduleRange1;
        private IScheduleRange _scheduleRange2;
        private ISameShiftBlockSpecification _target;
        private ITeamBlockInfo _teamBlockInfo;
        private ITeamInfo _teamInfo;
        private DateOnly _today;
        private IValidSampleDayPickerFromTeamBlock _validSampleDayPickerFromTeamBlock;

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
            _validSampleDayPickerFromTeamBlock = _mock.StrictMock<IValidSampleDayPickerFromTeamBlock>();
            _scheduleDayEquator = _mock.StrictMock<IScheduleDayEquator>();
            _scheduleDay1 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay2 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay3 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay4 = _mock.StrictMock<IScheduleDay>();
            _matrix1 = _mock.StrictMock<IScheduleMatrixPro>();
            _matrix2 = _mock.StrictMock<IScheduleMatrixPro>();
            _scheduleRange1 = _mock.StrictMock<IScheduleRange>();
            _scheduleRange2 = _mock.StrictMock<IScheduleRange>();
            _editableShift = _mock.StrictMock<IEditableShift>();
			_target = new SameShiftBlockSpecification(_validSampleDayPickerFromTeamBlock, _scheduleDayEquator);
        }

        private void commonExpect(DateOnlyPeriod dateOnlyPeriod, IEnumerable<IScheduleMatrixPro> matrixList)
        {
            Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
            Expect.Call(_blockInfo.BlockPeriod).Return(dateOnlyPeriod);
            Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
            Expect.Call(_teamInfo.MatrixesForGroupAndDate(_today)).Return(matrixList);
            Expect.Call(_scheduleDay1.GetEditorShift()).Return(_editableShift);
        }

        private void scheduleDayExpectCalls(IScheduleRange scheduleRange, IScheduleDay scheduleDay,
                                            SchedulePartView partValue, DateOnly dateOnly, bool result)
        {
            Expect.Call(scheduleRange.ScheduledDay(dateOnly)).Return(scheduleDay);
            Expect.Call(scheduleDay.SignificantPart()).Return(partValue);
            Expect.Call(scheduleDay.GetEditorShift()).Return(_editableShift);
            Expect.Call(_scheduleDayEquator.MainShiftEquals(_editableShift, _editableShift))
                  .IgnoreArguments()
                  .Return(result);
        }

        [Test]
        public void ShouldReturnTrueIfNoSampleDayIsFound()
        {
            using (_mock.Record())
            {
                Expect.Call(_validSampleDayPickerFromTeamBlock.GetSampleScheduleDay(_teamBlockInfo)).Return(null);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(_teamBlockInfo));
            }
        }

        [Test]
        public void ShouldReturnTrueIfNoMatrixFound()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(_today, _today);
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> {};

            using (_mock.Record())
            {
                Expect.Call(_validSampleDayPickerFromTeamBlock.GetSampleScheduleDay(_teamBlockInfo))
                      .Return(_scheduleDay1);

                commonExpect(dateOnlyPeriod, matrixList);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(_teamBlockInfo));
            }
        }

        [Test]
        public void ShouldReturnTrueIfSameShift()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(_today, _today);
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> {_matrix1};

            using (_mock.Record())
            {
                Expect.Call(_validSampleDayPickerFromTeamBlock.GetSampleScheduleDay(_teamBlockInfo))
                      .Return(_scheduleDay1);
                commonExpect(dateOnlyPeriod, matrixList);
                Expect.Call(_matrix1.ActiveScheduleRange).Return(_scheduleRange1);

                scheduleDayExpectCalls(_scheduleRange1, _scheduleDay1, SchedulePartView.MainShift, _today, true);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(_teamBlockInfo));
            }
        }

        [Test]
        public void ShouldReturnFalseIfDifferentStartTime()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(_today, _today.AddDays(1));
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> {_matrix1};
            using (_mock.Record())
            {
                Expect.Call(_validSampleDayPickerFromTeamBlock.GetSampleScheduleDay(_teamBlockInfo))
                      .Return(_scheduleDay1);
                commonExpect(dateOnlyPeriod, matrixList);
                Expect.Call(_matrix1.ActiveScheduleRange).Return(_scheduleRange1);

                scheduleDayExpectCalls(_scheduleRange1, _scheduleDay1, SchedulePartView.MainShift, _today, true);
                scheduleDayExpectCalls(_scheduleRange1, _scheduleDay2, SchedulePartView.MainShift, _today.AddDays(1),
                                       false);
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
            using (_mock.Record())
            {
                Expect.Call(_validSampleDayPickerFromTeamBlock.GetSampleScheduleDay(_teamBlockInfo))
                      .Return(_scheduleDay1);
                commonExpect(dateOnlyPeriod, matrixList);
                Expect.Call(_matrix1.ActiveScheduleRange).Return(_scheduleRange1);

                scheduleDayExpectCalls(_scheduleRange1, _scheduleDay1, SchedulePartView.MainShift, _today, true);
                scheduleDayExpectCalls(_scheduleRange1, _scheduleDay2, SchedulePartView.MainShift, _today.AddDays(1),
                                       true);

                Expect.Call(_matrix2.ActiveScheduleRange).Return(_scheduleRange2);
                scheduleDayExpectCalls(_scheduleRange2, _scheduleDay3, SchedulePartView.MainShift, _today, true);
                scheduleDayExpectCalls(_scheduleRange2, _scheduleDay4, SchedulePartView.MainShift, _today.AddDays(1),
                                       false);
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
            using (_mock.Record())
            {
                Expect.Call(_validSampleDayPickerFromTeamBlock.GetSampleScheduleDay(_teamBlockInfo))
                      .Return(_scheduleDay1);
                commonExpect(dateOnlyPeriod, matrixList);
                Expect.Call(_matrix1.ActiveScheduleRange).Return(_scheduleRange1);

                scheduleDayExpectCalls(_scheduleRange1, _scheduleDay1, SchedulePartView.MainShift, _today, true);
                scheduleDayExpectCalls(_scheduleRange1, _scheduleDay2, SchedulePartView.MainShift, _today.AddDays(1),
                                       true);

                Expect.Call(_matrix2.ActiveScheduleRange).Return(_scheduleRange2);
                scheduleDayExpectCalls(_scheduleRange2, _scheduleDay3, SchedulePartView.MainShift, _today, true);
                scheduleDayExpectCalls(_scheduleRange2, _scheduleDay4, SchedulePartView.MainShift, _today.AddDays(1),
                                       true);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(_teamBlockInfo));
            }
        }
    }
}