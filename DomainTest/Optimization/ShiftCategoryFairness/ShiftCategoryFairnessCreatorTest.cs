using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization.ShiftCategoryFairness
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
            
			IPersonAssignment assignment1 = PersonAssignmentFactory.CreatePersonAssignmentEmpty();
			IPersonAssignment assignment2 = PersonAssignmentFactory.CreatePersonAssignmentEmpty();
			IPersonAssignment assignment3 = PersonAssignmentFactory.CreatePersonAssignmentEmpty();

			var period = new DateTimePeriod(2013, 09, 12, 8, 2013, 09, 12, 9);
			
					assignment1.SetShiftCategory(fm);
					assignment1.AddActivity(new Activity("d"), period);
					assignment2.SetShiftCategory(fm);
					assignment2.AddActivity(new Activity("d"), period);
					assignment3.SetShiftCategory(da);
					assignment3.AddActivity(new Activity("d"), period);

            using (_mockRepository.Record())
            {
                Expect.Call(_scheduleRange.ScheduledDayCollection(_period))
                    .Return(_scheduleDays);
                Expect.Call(_d1.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_d1.PersonAssignment()).Return(assignment1);

                Expect.Call(_d2.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_d2.PersonAssignment()).Return(assignment2);

                Expect.Call(_d3.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_d3.PersonAssignment()).Return(assignment3);

            }
            IShiftCategoryFairnessHolder holder = _target.CreatePersonShiftCategoryFairness(_scheduleRange, _period);
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

            }
            IShiftCategoryFairnessHolder holder = _target.CreatePersonShiftCategoryFairness(_scheduleRange, _period);
            IDictionary<IShiftCategory, int> result = holder.ShiftCategoryFairnessDictionary;

            Assert.AreEqual(0, result.Keys.Count);
        }

    }
}