using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class ValidSampleDayPickerFromTeamBlockTest
    {
        private MockRepository _mocks;
        private IValidSampleDayPickerFromTeamBlock _target;
        private ITeamBlockInfo _teamBlockInfo;
        private IBlockInfo _blockInfo;
        private ITeamInfo _teamInfo;
        private DateOnly _today;
        private IScheduleMatrixPro _matrix1;
        private IScheduleRange _scheduleRange;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IScheduleDay _scheduleDay3;
        private IScheduleDay _scheduleDay4;
        private IScheduleRange _scheduleRange2;
        private IScheduleMatrixPro _matrix2;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new ValidSampleDayPickerFromTeamBlock();
            _teamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
            _blockInfo = _mocks.StrictMock<IBlockInfo>();
            _teamInfo = _mocks.StrictMock<ITeamInfo>();
            _today = new DateOnly(2013,10,31);
            _matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
            _matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
            _scheduleRange = _mocks.StrictMock<IScheduleRange>();
            _scheduleRange2 = _mocks.StrictMock<IScheduleRange>();
            _scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
            _scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
            _scheduleDay3 = _mocks.StrictMock<IScheduleDay>();
            _scheduleDay4 = _mocks.StrictMock<IScheduleDay>();
        }

        [Test]
        public void ShouldReturnNullIfNoMatrixFound()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(_today, _today);
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> {};
            using (_mocks.Record())
            {
                commonExpect(dateOnlyPeriod, matrixList);
            }
            using (_mocks.Playback())
            {
               Assert.IsNull(_target.GetSampleScheduleDay(_teamBlockInfo));
            }
        }

        [Test]
        public void ShouldReturnNullIfNoMainShiftFound()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(_today, _today);
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _matrix1};
            using (_mocks.Record())
            {
                commonExpect(dateOnlyPeriod, matrixList);
                
                Expect.Call(_matrix1.ActiveScheduleRange).Return(_scheduleRange);
                Expect.Call(_scheduleRange.ScheduledDay(_today)).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff);
            }
            using (_mocks.Playback())
            {
                Assert.IsNull(_target.GetSampleScheduleDay(_teamBlockInfo));
            }
        }

        [Test]
        public void ShouldReturnScheduleDayForSingleMatrixSingleDay()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(_today, _today);
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _matrix1 };
            using (_mocks.Record())
            {
                commonExpect(dateOnlyPeriod, matrixList);

                Expect.Call(_matrix1.ActiveScheduleRange).Return(_scheduleRange);
                Expect.Call(_scheduleRange.ScheduledDay(_today)).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
            }
            using (_mocks.Playback())
            {
                Assert.AreEqual( _target.GetSampleScheduleDay(_teamBlockInfo),_scheduleDay1 );
            }
        }

        [Test]
        public void ShouldReturnScheduleDayForSingleMatrixTwoDays()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(_today, _today.AddDays(1));
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _matrix1 };
            using (_mocks.Record())
            {
                commonExpect(dateOnlyPeriod, matrixList);

                Expect.Call(_matrix1.ActiveScheduleRange).Return(_scheduleRange);
                Expect.Call(_scheduleRange.ScheduledDay(_today)).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff );

               Expect.Call(_scheduleRange.ScheduledDay(_today.AddDays(1))).Return(_scheduleDay2);
                Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
            }
            using (_mocks.Playback())
            {
                Assert.AreEqual(_target.GetSampleScheduleDay(_teamBlockInfo), _scheduleDay2);
            }
        }

        [Test]
        public void ShouldReturnScheduleDayForTwoMatrixsTwoDays()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(_today, _today.AddDays(1));
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _matrix1,_matrix2 };
            using (_mocks.Record())
            {
                commonExpect(dateOnlyPeriod, matrixList);

                Expect.Call(_matrix1.ActiveScheduleRange).Return(_scheduleRange);
                
                Expect.Call(_scheduleRange.ScheduledDay(_today)).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff);

                Expect.Call(_scheduleRange.ScheduledDay(_today.AddDays(1))).Return(_scheduleDay2);
                Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.Absence);

                Expect.Call(_matrix2.ActiveScheduleRange).Return(_scheduleRange2);

                Expect.Call(_scheduleRange2.ScheduledDay(_today)).Return(_scheduleDay3);
                Expect.Call(_scheduleDay3.SignificantPart()).Return(SchedulePartView.DayOff);

                Expect.Call(_scheduleRange2.ScheduledDay(_today.AddDays(1))).Return(_scheduleDay4);
                Expect.Call(_scheduleDay4.SignificantPart()).Return(SchedulePartView.MainShift);
            }
            using (_mocks.Playback())
            {
                Assert.AreEqual(_target.GetSampleScheduleDay(_teamBlockInfo), _scheduleDay4);
            }
        }

        private void commonExpect(DateOnlyPeriod dateOnlyPeriod, IEnumerable<IScheduleMatrixPro> matrixList)
        {
            Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
            Expect.Call(_blockInfo.BlockPeriod).Return(dateOnlyPeriod);
            Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
            Expect.Call(_teamInfo.MatrixesForGroupAndDate(_today)).Return(matrixList);
        }
    }
}
