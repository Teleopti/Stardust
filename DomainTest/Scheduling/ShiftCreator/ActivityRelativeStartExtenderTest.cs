using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
    [TestFixture]
    public class ActivityRelativeStartExtenderTest
    {
        private ActivityRelativeStartExtender target;
        private Activity dummyActivity;

        [SetUp]
        public void Setup()
        {
            dummyActivity = new Activity("just dummy");
        }


        [Test]
        public void VerifyProtectedConstructor()
        {
            Assert.IsTrue(
                ReflectionHelper.HasDefaultConstructor(typeof(ActivityRelativeStartExtender)));
        }


        [Test]
        public void VerifyProtectedConstructorOnBaseType()
        {
            Assert.IsNotNull(new MockRepository().StrictMock<ActivityExtender>());
        }

        [Test]
        public void VerifyActivityMustNotBeNull()
        {
			Assert.Throws<ArgumentNullException>(() => target =
                new ActivityRelativeStartExtender(null, new TimePeriodWithSegment(1, 0, 1, 0, 1), new TimePeriodWithSegment(1, 0, 1, 0, 1)));
        }

        [Test]
        public void VerifyProcessShiftOneHour15MinutesStartSegmentFixedLength()
        {
            WorkShift originalShift = WorkShiftFactory.Create(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));

            target = new ActivityRelativeStartExtender(new Activity("for test"),
                                                       new TimePeriodWithSegment(0, 10, 0, 10, 15),
                                                       new TimePeriodWithSegment(1, 0, 2, 0, 15));

            IList<IWorkShift> retColl = target.ReplaceWithNewShifts(originalShift);

            Assert.AreEqual(5, retColl.Count);
            Assert.IsFalse(retColl.Contains(originalShift));

            assertPeriod(retColl, 1, new TimeSpan(10, 0, 0), new TimeSpan(10, 10, 0));
            assertPeriod(retColl, 1, new TimeSpan(10, 15, 0), new TimeSpan(10, 25, 0));
            assertPeriod(retColl, 1, new TimeSpan(10, 30, 0), new TimeSpan(10, 40, 0));
            assertPeriod(retColl, 1, new TimeSpan(10, 45, 0), new TimeSpan(10, 55, 0));
            assertPeriod(retColl, 1, new TimeSpan(11, 0, 0), new TimeSpan(11, 10, 0));
        }

        [Test]
        public void VerifyProperties()
        {
            target = new ActivityRelativeStartExtender(dummyActivity,
                                                       new TimePeriodWithSegment(0, 10, 0, 30, 15),
                                                       new TimePeriodWithSegment(1, 0, 2, 0, 15));
            Assert.AreSame(dummyActivity, target.ExtendWithActivity);
            Assert.AreEqual(new TimePeriodWithSegment(0, 10, 0, 30, 15), target.ActivityLengthWithSegment);
            Assert.AreEqual(new TimePeriodWithSegment(1, 0, 2, 0, 15), target.ActivityPositionWithSegment);
            Assert.AreEqual(new TimeSpan(0,30,0), target.ExtendMaximum());
        }

        [Test]
        public void CanSetProperties()
        {
            target = new ActivityRelativeStartExtender(dummyActivity, new TimePeriodWithSegment(), new TimePeriodWithSegment());
            Activity newAct = new Activity("sdf");
            TimePeriodWithSegment actLength = new TimePeriodWithSegment(10, 0, 20, 0, 15);
            TimePeriodWithSegment actPos = new TimePeriodWithSegment(13, 0, 21, 0, 20);
            target.ActivityPositionWithSegment = actPos;
            target.ExtendWithActivity = newAct;
            target.ActivityLengthWithSegment = actLength;
            Assert.AreSame(newAct, target.ExtendWithActivity);
            Assert.AreEqual(actLength, target.ActivityLengthWithSegment);
            Assert.AreEqual(actPos, target.ActivityPositionWithSegment);
        }

        [Test]
        public void CannotSetActivityToNull()
        {
			Assert.Throws<ArgumentNullException>(() => target = new ActivityRelativeStartExtender(dummyActivity, new TimePeriodWithSegment(),
		        new TimePeriodWithSegment()) {ExtendWithActivity = null});
        }


        [Test]
        public void VerifyProcessShiftBetween60MinutesAnd70Minutes5MinutesLengthSegment()
        {
            WorkShift originalShift = WorkShiftFactory.Create(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));
            target = new ActivityRelativeStartExtender(new Activity("for test"),
                                                       new TimePeriodWithSegment(0, 60, 0, 70, 5),
                                                       new TimePeriodWithSegment(1, 0, 1, 5, 5));

            IList<IWorkShift> retColl = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(6, retColl.Count);

            assertPeriod(retColl, 1, new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0));
            assertPeriod(retColl, 1, new TimeSpan(10, 0, 0), new TimeSpan(11, 5, 0));
            assertPeriod(retColl, 1, new TimeSpan(10, 0, 0), new TimeSpan(11, 10, 0));
            assertPeriod(retColl, 1, new TimeSpan(10, 5, 0), new TimeSpan(11, 5, 0));
            assertPeriod(retColl, 1, new TimeSpan(10, 5, 0), new TimeSpan(11, 10, 0));
            assertPeriod(retColl, 1, new TimeSpan(10, 5, 0), new TimeSpan(11, 15, 0));
        }

        [Test]
        public void VerifyDifferentSegmentsOnActivityAndStart()
        {
            WorkShift originalShift = WorkShiftFactory.Create(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));
            target = new ActivityRelativeStartExtender(new Activity("for test"),
                                                       new TimePeriodWithSegment(0, 60, 0, 65, 5),
                                                       new TimePeriodWithSegment(1, 0, 1, 11, 10));
            IList<IWorkShift> retColl = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(4, retColl.Count);
            assertPeriod(retColl, 1, new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0));
            assertPeriod(retColl, 1, new TimeSpan(10, 0, 0), new TimeSpan(11, 5, 0));
            assertPeriod(retColl, 1, new TimeSpan(10, 10, 0), new TimeSpan(11, 10, 0));
            assertPeriod(retColl, 1, new TimeSpan(10, 10, 0), new TimeSpan(11, 15, 0));
        }

        [Test]
        public void VerifyProcessShiftNotAddOutsideScope()
        {
            WorkShift originalShift = WorkShiftFactory.Create(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));
            target = new ActivityRelativeStartExtender(new Activity("for test"),
                                                       new TimePeriodWithSegment(0, 10, 1, 0, 15),
                                                       new TimePeriodWithSegment(7, 50, 9, 0, 15));

            IList<IWorkShift> retColl = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(1, retColl.Count);
            assertPeriod(retColl, 1, new TimeSpan(16, 50, 0), new TimeSpan(17, 0, 0));
        }


        [Test]
        public void VerifyProcessShiftNotAddAfter()
        {
            WorkShift originalShift = WorkShiftFactory.Create(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));

            target = new ActivityRelativeStartExtender(new Activity("for test"),
                                                       new TimePeriodWithSegment(0, 10, 1, 0, 15),
                                                       new TimePeriodWithSegment(7, 51, 9, 0, 15));
            IList<IWorkShift> retColl = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(0, retColl.Count);
        }

        [Test]
        public void VerifyProcessShiftWorkShiftMustNotBeNull()
        {
            target = new ActivityRelativeStartExtender(new Activity("for test"),
                                                       new TimePeriodWithSegment(0, 10, 0, 10, 15),
                                                       new TimePeriodWithSegment(1, 0, 2, 0, 15));
			Assert.Throws<ArgumentNullException>(() => target.ReplaceWithNewShifts(null));
        }


        [Test]
        public void CannotExtendOutsideShift()
        {
            WorkShift originalShift = WorkShiftFactory.Create(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0));
            target = new ActivityRelativeStartExtender(new Activity("for test"),
                                           new TimePeriodWithSegment(1, 0, 1, 0, 15),
                                           new TimePeriodWithSegment(2, 0, 2, 0, 15));
            IList<IWorkShift> retColl = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(0, retColl.Count);
        }


        [Test]
        public void VerifyPossibleStart()
        {
            ActivityStartDefinitionForTest target2 = new ActivityStartDefinitionForTest(dummyActivity,
                                                                                        new TimePeriodWithSegment(0, 15, 0, 59, 15),
                                                                                        new TimePeriodWithSegment(2, 0, 3, 0, 15));
            DateTimePeriod period = new DateTimePeriod(
                WorkShift.BaseDate.Add(new TimeSpan(8, 0, 0)), WorkShift.BaseDate.Add(new TimeSpan(17, 0, 0)));

            DateTimePeriod periodToExpect = new DateTimePeriod(
                WorkShift.BaseDate.Add(new TimeSpan(10, 0, 0)), WorkShift.BaseDate.Add(new TimeSpan(11, 0, 0)));

            Assert.AreEqual(periodToExpect, target2.exposePossiblePeriod(period));
        }

        internal class ActivityStartDefinitionForTest : ActivityRelativeStartExtender
        {
            internal ActivityStartDefinitionForTest(Activity activity, TimePeriodWithSegment activityLength,
                                                    TimePeriodWithSegment start)
                : base(activity, activityLength, start)
            {
            }

            internal DateTimePeriod exposePossiblePeriod(DateTimePeriod templateProjectionPeriod)
            {
                return base.PossiblePeriodForActivity(templateProjectionPeriod).Value;
            }
        }

        private static void assertPeriod(IEnumerable<IWorkShift> shiftColl, 
                                         int layerIndex, 
                                         TimeSpan start, 
                                         TimeSpan end)
        {
            bool existFlag=false;
            foreach (IWorkShift shift in shiftColl)
            {
                if (shift.LayerCollection[layerIndex].Period.Equals(new DateTimePeriod(WorkShift.BaseDate.Add(start), WorkShift.BaseDate.Add(end))))
                {
                    existFlag = true;
                    break;
                }
            }
            Assert.IsTrue(existFlag);
        }
    }
}