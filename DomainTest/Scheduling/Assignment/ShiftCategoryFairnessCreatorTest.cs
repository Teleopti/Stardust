﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class ShiftCategoryFairnessCreatorTest
    {
        private ShiftCategoryFairnessCreator _target;
        private IScheduleRange _scheduleRange;
        private DateOnlyPeriod _period;
        private MockRepository _mockRepository;
        private IList<IScheduleDay> _scheduleDays;

        private IScheduleDay _d1;
        private IScheduleDay _d2;
        private IScheduleDay _d3;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _target = new ShiftCategoryFairnessCreator();
            _scheduleRange = _mockRepository.StrictMock<IScheduleRange>();
            _period = new DateOnlyPeriod();

            _d1 = _mockRepository.StrictMock<IScheduleDay>();
            _d2 = _mockRepository.StrictMock<IScheduleDay>();
            _d3 = _mockRepository.StrictMock<IScheduleDay>();

            _scheduleDays = new List<IScheduleDay>{ _d1, _d2, _d3 };
        }

        [Test]
        public void VerifyCreateShiftCategoryFairnessDictionary()
        {
            IShiftCategory fm = new ShiftCategory("FM");
            IShiftCategory da = new ShiftCategory("DA");
            
            IMainShift fm1 = new MainShift(fm);
            IMainShift fm2 = new MainShift(fm);
            IMainShift da1 = new MainShift(da);

            IPersonAssignment assignment1 = PersonAssignmentFactory.CreatePersonAssignmentEmpty();
            assignment1.SetMainShift(fm1);
            
            IPersonAssignment assignment2 = PersonAssignmentFactory.CreatePersonAssignmentEmpty();
            assignment2.SetMainShift(fm2);

            IPersonAssignment assignment3 = PersonAssignmentFactory.CreatePersonAssignmentEmpty();
            assignment3.SetMainShift(da1);

            using (_mockRepository.Record())
            {
                Expect.Call(_scheduleRange.ScheduledDayCollection(_period))
                    .Return(_scheduleDays);
                Expect.Call(_d1.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_d1.AssignmentHighZOrder()).Return(assignment1);

                Expect.Call(_d2.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_d2.AssignmentHighZOrder()).Return(assignment2);

                Expect.Call(_d3.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_d3.AssignmentHighZOrder()).Return(assignment3);

                Expect.Call(_scheduleRange.FairnessPoints())
                    .Return(new FairnessValueResult())
                    .Repeat.AtLeastOnce();

            }
            IShiftCategoryFairness holder = _target.CreatePersonShiftCategoryFairness(_scheduleRange, _period);
            IDictionary<IShiftCategory, int> result = holder.ShiftCategoryFairnessDictionary;

            Assert.AreEqual(2, result.Keys.Count);
            Assert.AreEqual(2, result[fm]);
            Assert.AreEqual(1, result[da]);
        }

        [Test]
        public void VerifyCreateShiftCategoryFairnessDictionaryWithNullMainShift()
        {
            using (_mockRepository.Record())
            {
                Expect.Call(_scheduleRange.ScheduledDayCollection(_period))
                    .Return(_scheduleDays);
                Expect.Call(_d1.SignificantPart()).Return(SchedulePartView.DayOff);
                Expect.Call(_d2.SignificantPart()).Return(SchedulePartView.FullDayAbsence);
                Expect.Call(_d3.SignificantPart()).Return(SchedulePartView.None);
                Expect.Call(_scheduleRange.FairnessPoints())
                    .Return(new FairnessValueResult());

            }
            IShiftCategoryFairness holder = _target.CreatePersonShiftCategoryFairness(_scheduleRange, _period);
            IDictionary<IShiftCategory, int> result = holder.ShiftCategoryFairnessDictionary;

            Assert.AreEqual(0, result.Keys.Count);
        }

    }
}