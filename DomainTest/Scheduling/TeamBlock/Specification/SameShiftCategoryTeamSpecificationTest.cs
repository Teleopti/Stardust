﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Specification
{
    [TestFixture]
    public class SameShiftCategoryTeamSpecificationTest
    {
		private MockRepository _mock;
		private ISameShiftCategoryTeamSpecification _target;
        private ITeamInfo _teamInfo;
        private IBlockInfo _blockInfo;
        private IList<IScheduleMatrixPro> _matrixList;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private DateOnly _today;
        private ITeamBlockInfo _teamBlockInfo;
        private IScheduleDay _scheduleDay1;
        private IPersonAssignment _personAssignment1;
        private IPersonAssignment _personAssignment2;
        private IShiftCategory _shiftcategory1;
        private IShiftCategory _shiftcategory2;
        private IScheduleMatrixPro _matrix1;
        private IScheduleRange _scheduleRange1;
        private IScheduleDay _scheduleDay2;
        private IScheduleMatrixPro _matrix2;
        private IScheduleRange _scheduleRange2;
        private IScheduleDay _scheduleDay3;
        private IScheduleDay _scheduleDay4;

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
			_target = new SameShiftCategoryTeamSpecification();
			_scheduleDay1 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay3 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay4 = _mock.StrictMock<IScheduleDay>();
			_personAssignment1 = _mock.StrictMock<IPersonAssignment>();
			_personAssignment2 = _mock.StrictMock<IPersonAssignment>();
			_shiftcategory1 = new ShiftCategory("test1");
			_shiftcategory2 = new ShiftCategory("test2");
			_matrix1 = _mock.StrictMock<IScheduleMatrixPro>();
			_matrix2 = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleRange1 = _mock.StrictMock<IScheduleRange>();
			_scheduleRange2 = _mock.StrictMock<IScheduleRange>();
		}

        [Test]
        public void ShouldReturnTrueForSameShiftCategoryInTeam()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(_today, _today);
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> {_matrix1, _matrix2};

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
				Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
				Expect.Call(_personAssignment1.ShiftCategory).Return(_shiftcategory1);
				
				Expect.Call(_scheduleRange2.ScheduledDay(_today)).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.PersonAssignment()).Return(_personAssignment2);
				Expect.Call(_personAssignment2.ShiftCategory).Return(_shiftcategory1);

            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(_teamBlockInfo));
            }
        }

        [Test]
        public void ShouldReturnFalseIfDifferentShiftCategory()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(_today, _today);
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> {_matrix1, _matrix2};

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
				Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
				Expect.Call(_personAssignment1.ShiftCategory).Return(_shiftcategory1);

				Expect.Call(_scheduleRange2.ScheduledDay(_today)).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.PersonAssignment()).Return(_personAssignment2);
				Expect.Call(_personAssignment2.ShiftCategory).Return(_shiftcategory2);
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
            IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro>();

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
		public void ShouldBeTrueWhenTwoDaysHaveDifferentShiftCategories()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(_today, _today.AddDays(1));
			IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _matrix1, _matrix2 };
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
				Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
				Expect.Call(_personAssignment1.ShiftCategory).Return(_shiftcategory1);

				Expect.Call(_scheduleRange1.ScheduledDay(_today.AddDays(1))).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.PersonAssignment()).Return(_personAssignment2);
				Expect.Call(_personAssignment2.ShiftCategory).Return(_shiftcategory2);
				
				Expect.Call(_scheduleRange2.ScheduledDay(_today)).Return(_scheduleDay3);
				Expect.Call(_scheduleDay3.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay3.PersonAssignment()).Return(_personAssignment1);
				Expect.Call(_personAssignment1.ShiftCategory).Return(_shiftcategory1);

				Expect.Call(_scheduleRange2.ScheduledDay(_today.AddDays(1))).Return(_scheduleDay4);
				Expect.Call(_scheduleDay4.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay4.PersonAssignment()).Return(_personAssignment2);
				Expect.Call(_personAssignment2.ShiftCategory).Return(_shiftcategory2);
			}
			using (_mock.Playback())
			{
				Assert.IsTrue(_target.IsSatisfiedBy(_teamBlockInfo));
			}
		}
    }
}