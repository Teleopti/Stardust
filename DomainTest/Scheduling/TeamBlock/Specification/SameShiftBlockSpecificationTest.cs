using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Specification
{
    public class SameShiftBlockSpecificationTest
    {
        private IBlockInfo _blockInfo;
        private IEditableShift _editableShift;
        private IList<IScheduleMatrixPro> _matrixList;
        private MockRepository _mock;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IScheduleDayEquator _scheduleDayEquator;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private IScheduleRange _scheduleRange1;
        private ISameShiftBlockSpecification _target;
        private ITeamBlockInfo _teamBlockInfo;
        private ITeamInfo _teamInfo;
        private DateOnly _today;
        private IValidSampleDayPickerFromTeamBlock _validSampleDayPickerFromTeamBlock;
	    private IPerson _person;
			
			
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
            _scheduleRange1 = _mock.StrictMock<IScheduleRange>();
            _editableShift = _mock.StrictMock<IEditableShift>();
			_target = new SameShiftBlockSpecification(_validSampleDayPickerFromTeamBlock, _scheduleDayEquator);
	        _person = PersonFactory.CreatePerson();
        }

        [Test]
        public void ShouldReturnTrueIfNoSampleDayIsFound()
        {
            using (_mock.Record())
            {
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_blockInfo.BlockPeriod).Return(new DateOnlyPeriod(_today,_today));
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(_today)).Return(_matrixList);
				Expect.Call(_scheduleMatrixPro.Person).Return(_person);
                Expect.Call(_validSampleDayPickerFromTeamBlock.GetSampleScheduleDay(_teamBlockInfo, _person)).Return(null);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(_teamBlockInfo));
            }
        }

        [Test]
        public void ShouldReturnTrueIfNoMatrixFound()
        {

            using (_mock.Record())
            {
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_blockInfo.BlockPeriod).Return(new DateOnlyPeriod(_today, _today));
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(_today)).Return(new List<IScheduleMatrixPro>());
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(_teamBlockInfo));
            }
        }

        [Test]
        public void ShouldReturnTrueIfSameShift()
        {
            using (_mock.Record())
            {
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_blockInfo.BlockPeriod).Return(new DateOnlyPeriod(_today, _today));
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(_today)).Return(_matrixList);
				Expect.Call(_scheduleMatrixPro.Person).Return(_person);
				Expect.Call(_validSampleDayPickerFromTeamBlock.GetSampleScheduleDay(_teamBlockInfo, _person)).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.GetEditorShift()).Return(_editableShift);
				Expect.Call(_scheduleMatrixPro.ActiveScheduleRange).Return(_scheduleRange1);
				Expect.Call(_scheduleRange1.ScheduledDay(_today)).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.GetEditorShift()).Return(_editableShift);
				Expect.Call(_scheduleDay1.TimeZone).Return(TimeZoneInfo.Utc);
				Expect.Call(_scheduleDayEquator.MainShiftBasicEquals(_editableShift, _editableShift, TimeZoneInfo.Utc)).Return(true);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(_teamBlockInfo));
            }
        }

        [Test]
        public void ShouldReturnFalseIfDifferentStartTime()
        {
            using (_mock.Record())
            {
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_blockInfo.BlockPeriod).Return(new DateOnlyPeriod(_today, _today));
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(_today)).Return(_matrixList);
				Expect.Call(_scheduleMatrixPro.Person).Return(_person);
				Expect.Call(_validSampleDayPickerFromTeamBlock.GetSampleScheduleDay(_teamBlockInfo, _person)).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.GetEditorShift()).Return(_editableShift);
				Expect.Call(_scheduleMatrixPro.ActiveScheduleRange).Return(_scheduleRange1);
				Expect.Call(_scheduleRange1.ScheduledDay(_today)).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.GetEditorShift()).Return(_editableShift);
				Expect.Call(_scheduleDay1.TimeZone).Return(TimeZoneInfo.Utc);
				Expect.Call(_scheduleDayEquator.MainShiftBasicEquals(_editableShift, _editableShift, TimeZoneInfo.Utc)).Return(false);
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(_teamBlockInfo));
            }
        }
    }
}