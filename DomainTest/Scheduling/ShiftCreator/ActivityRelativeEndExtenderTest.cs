using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
    [TestFixture]
    public class ActivityRelativeEndExtenderTest
    {
        private Activity dummyActivity;

        [SetUp]
        public void Setup()
        {
            dummyActivity = new Activity("for test");
        }

        [Test]
        public void VerifyProtectedConstructor()
        {
            Assert.IsTrue(
                ReflectionHelper.HasDefaultConstructor(typeof(ActivityRelativeEndExtender)));
        }

        [Test]
        public void VerifyPossiblePeriod()
        {
            ActivityEndDefinitionForTest target2 = new ActivityEndDefinitionForTest(dummyActivity,
                                                                                    new TimePeriodWithSegment(0, 15, 0, 59, 15),
                                                                                    new TimePeriodWithSegment(1, 0, 2, 0, 15));
            DateTimePeriod period = new DateTimePeriod(
                WorkShift.BaseDate.Add(new TimeSpan(8, 0, 0)), WorkShift.BaseDate.Add(new TimeSpan(17, 0, 0)));

            DateTimePeriod periodToExpect = new DateTimePeriod(
                WorkShift.BaseDate.Add(new TimeSpan(15, 0, 0)), WorkShift.BaseDate.Add(new TimeSpan(16, 0, 0)));

            Assert.AreEqual(periodToExpect, target2.exposePossiblePeriod(period));
        }


        [Test]
        public void VerifyProcessShiftNotAddOutsideScope()
        {
            WorkShift originalShift = WorkShiftFactory.Create(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));
            ActivityEndDefinitionForTest target = new ActivityEndDefinitionForTest(new Activity("for test"),
                                                                                   new TimePeriodWithSegment(0, 10, 0, 10, 15),
                                                                                   new TimePeriodWithSegment(7, 55, 9, 0, 10));

            IList<IWorkShift> retColl = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(1, retColl.Count);
            DateTimePeriod expectedPeriod = new DateTimePeriod(
                WorkShift.BaseDate.Add(new TimeSpan(9,0,0)), 
                WorkShift.BaseDate.Add(new TimeSpan(9,10,0)));
            Assert.AreEqual(expectedPeriod, retColl[0].LayerCollection[1].Period);
        }

        [Test]
        public void VerifyRelativeEndWithSameEarlyAndLateStart()
        {
            WorkShift originalShift = WorkShiftFactory.Create(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));
            ActivityEndDefinitionForTest target = new ActivityEndDefinitionForTest(dummyActivity,
                                                                       new TimePeriodWithSegment(1, 0, 1, 0, 15),
                                                                       new TimePeriodWithSegment(1, 0, 1, 0, 10));
            var retColl = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(1, retColl.Count);
        }


        [Test]
        public void CannotExtendOutsideShift()
        {
            WorkShift originalShift = WorkShiftFactory.Create(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0));
            ActivityEndDefinitionForTest target = new ActivityEndDefinitionForTest(new Activity("for test"),
                                                                                    new TimePeriodWithSegment(0, 10, 0, 10, 15),
                                                                                    new TimePeriodWithSegment(1, 10, 2, 0, 10));
            IList<IWorkShift> retColl = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(0, retColl.Count);
        }


        internal class ActivityEndDefinitionForTest : ActivityRelativeEndExtender
        {
            internal ActivityEndDefinitionForTest(Activity activity, TimePeriodWithSegment activityLength,
                                                  TimePeriodWithSegment start)
                : base(activity, activityLength, start)
            {
            }

            internal DateTimePeriod exposePossiblePeriod(DateTimePeriod templateProjectionPeriod)
            {
                return base.PossiblePeriodForActivity(templateProjectionPeriod).Value;
            }
        }
    }
}