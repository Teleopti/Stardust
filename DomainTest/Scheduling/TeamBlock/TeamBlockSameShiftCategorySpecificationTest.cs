using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class TeamBlockSameShiftCategorySpecificationTest
    {
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
            _target = new TeamBlockSameShiftCategorySpecification(_validSampleDayPickerFromTeamBlock);
            _scheduleDay1 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay2 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay3 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay4 = _mock.StrictMock<IScheduleDay>();
            _personAssignment = _mock.StrictMock<IPersonAssignment>();
            _personAssignment2 = _mock.StrictMock<IPersonAssignment>();
            _shiftcategory = new ShiftCategory("test");
            _matrix1 = _mock.StrictMock<IScheduleMatrixPro>();
            _matrix2 = _mock.StrictMock<IScheduleMatrixPro>();
            _scheduleRange1 = _mock.StrictMock<IScheduleRange>();
            _scheduleRange2 = _mock.StrictMock<IScheduleRange>();
        }

        private MockRepository _mock;
        private ITeamBlockSameShiftCategorySpecification _target;
        private ITeamInfo _teamInfo;
        private IBlockInfo _blockInfo;
        private IList<IScheduleMatrixPro> _matrixList;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private DateOnly _today;
        private ITeamBlockInfo _teamBlockInfo;
        private IValidSampleDayPickerFromTeamBlock _validSampleDayPickerFromTeamBlock;
        private IScheduleDay _scheduleDay1;
        private IPersonAssignment _personAssignment;
        private IShiftCategory _shiftcategory;
        private IScheduleMatrixPro _matrix1;
        private IScheduleRange _scheduleRange1;
        private IPersonAssignment _personAssignment2;
        private IScheduleDay _scheduleDay2;
        private IScheduleMatrixPro _matrix2;
        private IScheduleRange _scheduleRange2;
        private IScheduleDay _scheduleDay3;
        private IScheduleDay _scheduleDay4;

        private void commonExpect(DateOnlyPeriod dateOnlyPeriod, IEnumerable<IScheduleMatrixPro> matrixList)
        {
            Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
            Expect.Call(_blockInfo.BlockPeriod).Return(dateOnlyPeriod);
            Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
            Expect.Call(_teamInfo.MatrixesForGroupAndDate(_today)).Return(matrixList);
            Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment);
            Expect.Call(_personAssignment.ShiftCategory).Return(_shiftcategory);
        }

        private void scheduleDayExpectCalls(IScheduleRange scheduleRange, IScheduleDay scheduleDay,
                                            SchedulePartView partValue, IPersonAssignment personAssignment,
                                            IShiftCategory shiftcategory, DateOnly dateOnly)
        {
            Expect.Call(scheduleRange.ScheduledDay(dateOnly)).Return(scheduleDay);
            Expect.Call(scheduleDay.SignificantPart()).Return(partValue);
            Expect.Call(scheduleDay.PersonAssignment()).Return(personAssignment);
            Expect.Call(personAssignment.ShiftCategory).Return(shiftcategory);
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

                scheduleDayExpectCalls(_scheduleRange1, _scheduleDay1, SchedulePartView.MainShift, _personAssignment,
                                       _shiftcategory, _today);
                scheduleDayExpectCalls(_scheduleRange1, _scheduleDay2, SchedulePartView.MainShift, _personAssignment2,
                                       _shiftcategory, _today.AddDays(1));

                Expect.Call(_matrix2.ActiveScheduleRange).Return(_scheduleRange2);
                scheduleDayExpectCalls(_scheduleRange2, _scheduleDay3, SchedulePartView.MainShift, _personAssignment,
                                       _shiftcategory, _today);
                scheduleDayExpectCalls(_scheduleRange2, _scheduleDay4, SchedulePartView.MainShift, _personAssignment2,
                                       new ShiftCategory("wf"), _today.AddDays(1));
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(_teamBlockInfo));
            }
        }

        [Test]
        public void ShouldReturnFalseIfDifferentShiftCategory()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(_today, _today.AddDays(1));
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> {_matrix1};

            using (_mock.Record())
            {
                Expect.Call(_validSampleDayPickerFromTeamBlock.GetSampleScheduleDay(_teamBlockInfo))
                      .Return(_scheduleDay1);
                commonExpect(dateOnlyPeriod, matrixList);
                Expect.Call(_matrix1.ActiveScheduleRange).Return(_scheduleRange1);

                scheduleDayExpectCalls(_scheduleRange1, _scheduleDay1, SchedulePartView.MainShift, _personAssignment,
                                       _shiftcategory, _today);
                scheduleDayExpectCalls(_scheduleRange1, _scheduleDay2, SchedulePartView.MainShift, _personAssignment2,
                                       new ShiftCategory("erg"), _today.AddDays(1));
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(_teamBlockInfo));
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
        public void ShouldReturnTrueIfSameShiftCategory()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(_today, _today);
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> {_matrix1};

            using (_mock.Record())
            {
                Expect.Call(_validSampleDayPickerFromTeamBlock.GetSampleScheduleDay(_teamBlockInfo))
                      .Return(_scheduleDay1);
                commonExpect(dateOnlyPeriod, matrixList);
                Expect.Call(_matrix1.ActiveScheduleRange).Return(_scheduleRange1);

                scheduleDayExpectCalls(_scheduleRange1, _scheduleDay1, SchedulePartView.MainShift, _personAssignment,
                                       _shiftcategory, _today);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(_teamBlockInfo));
            }
        }
    }
}