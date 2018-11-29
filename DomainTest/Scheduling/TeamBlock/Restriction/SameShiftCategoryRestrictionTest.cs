using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Restriction
{
    [TestFixture]
    public class SameShiftCategoryRestrictionTest
    {
        private MockRepository _mocks;
        private IScheduleRestrictionStrategy _target;
        private SchedulingOptions _schedulingOptions;
        private DateOnly _dateOnly;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private IScheduleDay _scheduleDay1;
        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDay _scheduleDay2;
        private IScheduleDayPro _scheduleDayPro2;
        private IShiftCategory _shiftCategory;
        private IPersonAssignment _personAssignment;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _shiftCategory = new ShiftCategory("cat");
            _schedulingOptions = new SchedulingOptions();
				_schedulingOptions.UseBlock = true;
            _target = new SameShiftCategoryRestriction();
            _dateOnly = new DateOnly(2012, 12, 7);
            _scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
            _scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
            _scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
            _scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
            _personAssignment = _mocks.StrictMock<IPersonAssignment>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldExtractSameShiftCategoryRestrictionFromScheduleDay()
        {
            _schedulingOptions.BlockSameShiftCategory = true;
            var dateList = new List<DateOnly> { _dateOnly, _dateOnly.AddDays(1) };
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro1);
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly.AddDays(1))).Return(_scheduleDayPro2);
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment);
                Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_scheduleDay2.PersonAssignment()).Return(_personAssignment);
                Expect.Call(_personAssignment.ShiftCategory).Return(_shiftCategory).Repeat.Twice();
            }
            using (_mocks.Playback())
            {
                var expected = new EffectiveRestriction(new StartTimeLimitation(),
                                                        new EndTimeLimitation(),
                                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>())
                {
                    ShiftCategory = _shiftCategory
                };

                var result = _target.ExtractRestriction (dateList, matrixList);

                Assert.That(result, Is.EqualTo(expected));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldExtractSameShiftCategoryRestrictionFromOnePersonOneBlock()
        {
            _schedulingOptions.BlockSameShiftCategory = true;
            var dateList = new List<DateOnly> { _dateOnly, _dateOnly.AddDays(1) };
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro1);
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly.AddDays(1))).Return(_scheduleDayPro2);
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment);
                Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_scheduleDay2.PersonAssignment()).Return(_personAssignment);
                Expect.Call(_personAssignment.ShiftCategory).Return(_shiftCategory).Repeat.Twice();
            }
            using (_mocks.Playback())
            {
                var expected = new EffectiveRestriction(new StartTimeLimitation(),
                                                        new EndTimeLimitation(),
                                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>())
                {
                    ShiftCategory = _shiftCategory
                };

                var result = _target.ExtractRestriction(dateList, matrixList);

                Assert.That(result, Is.EqualTo(expected));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldExtractSameShiftCategoryRestrictionFromOneTeamOneDay()
        {
			  _schedulingOptions.UseTeam = true;
            _schedulingOptions.TeamSameShiftCategory = true;
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro1);
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment);
                Expect.Call(_personAssignment.ShiftCategory).Return(_shiftCategory);
            }
            using (_mocks.Playback())
            {
                var expected = new EffectiveRestriction(new StartTimeLimitation(),
                                                        new EndTimeLimitation(),
                                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>())
                {
                    ShiftCategory = _shiftCategory
                };

                var result = _target.ExtractRestriction(new List<DateOnly> { _dateOnly }, matrixList);

                Assert.That(result, Is.EqualTo(expected));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldNotExtractDifferentShiftCategoryRestrictionFromScheduleDay()
        {
            _schedulingOptions.BlockSameShiftCategory = true;
            var dateList = new List<DateOnly> { _dateOnly, _dateOnly.AddDays(1) };
            var personAssignment2 = _mocks.StrictMock<IPersonAssignment>();
            var shiftCategory2 = new ShiftCategory("cat2");
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
													   new EndTimeLimitation(),
													   new WorkTimeLimitation(), null, null, null,
													   new List<IActivityRestriction>());
            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro1);
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly.AddDays(1))).Return(_scheduleDayPro2);
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment);
                Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_scheduleDay2.PersonAssignment()).Return(personAssignment2);
                Expect.Call(_personAssignment.ShiftCategory).Return(_shiftCategory);
                Expect.Call(personAssignment2.ShiftCategory).Return(shiftCategory2);
            }
            using (_mocks.Playback())
            {
                var result = _target.ExtractRestriction(dateList, matrixList);

	            Assert.That(result, Is.EqualTo(restriction));
            }
        }

        [Test]
        public void ShouldExtractSameShiftCategoryWhenPersonAssignmentIsEmpty()
        {
            _schedulingOptions.BlockSameShiftCategory = true;
            var dateList = new List<DateOnly> { _dateOnly };
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro1);
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(null);
            }
            using (_mocks.Playback())
            {
                var expected = new EffectiveRestriction(new StartTimeLimitation(),
                                                       new EndTimeLimitation(),
                                                       new WorkTimeLimitation(), null, null, null,
                                                       new List<IActivityRestriction>());
                var result = _target.ExtractRestriction(dateList, matrixList);
                Assert.That(result, Is.EqualTo(expected));
            }
        }

        [Test]
        public void ShouldExtractSameShiftCategoryWhenMainShiftIsNull()
        {
            _schedulingOptions.BlockSameShiftCategory = true;
            var dateList = new List<DateOnly> { _dateOnly };
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro1);
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment);
                Expect.Call(_personAssignment.ShiftCategory).Return(null);
            }
            using (_mocks.Playback())
            {
                var expected = new EffectiveRestriction(new StartTimeLimitation(),
                                                       new EndTimeLimitation(),
                                                       new WorkTimeLimitation(), null, null, null,
                                                       new List<IActivityRestriction>());
                var result = _target.ExtractRestriction(dateList, matrixList);
                Assert.That(result, Is.EqualTo(expected));
            }
        }

        [Test]
        public void ShouldExtractSameShiftCategoryWhenScheduleIsNull()
        {
            _schedulingOptions.BlockSameShiftCategory = true;
            var dateList = new List<DateOnly> { _dateOnly };
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(null);
            }
            using (_mocks.Playback())
            {
                var expected = new EffectiveRestriction(new StartTimeLimitation(),
                                                       new EndTimeLimitation(),
                                                       new WorkTimeLimitation(), null, null, null,
                                                       new List<IActivityRestriction>());
                var result = _target.ExtractRestriction(dateList, matrixList);
                Assert.That(result, Is.EqualTo(expected));
            }
        }
    }
}
