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
    /// <summary>
    /// Most of the tests are indirectly made
    /// in ActivityRelativeStartDefinitionTest.
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-03-05
    /// </remarks>
    [TestFixture]
    public class ActivityAbsoluteStartExtenderTest
    {
        private Activity dummyActivity;

        [SetUp]
        public void Setup()
        {
            dummyActivity = new Activity("for test");
        }

        [Test]
        public void VerifyPossibleStart()
        {
            ActivityStartDefinitionForTest target2 = new ActivityStartDefinitionForTest(dummyActivity,
                                                                                        new TimePeriodWithSegment(0, 15, 0, 59, 15),
                                                                                        new TimePeriodWithSegment(11, 0, 12, 0, 15));
            DateTimePeriod period = new DateTimePeriod(
                WorkShift.BaseDate.Add(new TimeSpan(8, 0, 0)), WorkShift.BaseDate.Add(new TimeSpan(17, 0, 0)));

            DateTimePeriod periodToExpect = new DateTimePeriod(
                WorkShift.BaseDate.Add(new TimeSpan(11, 0, 0)), WorkShift.BaseDate.Add(new TimeSpan(12, 0, 0)));

            Assert.AreEqual(periodToExpect, target2.exposePossiblePeriod(period));
        }


        [Test]
        public void VerifyProcessShiftNotAddOutsideScope()
        {
            WorkShift originalShift = WorkShiftFactory.Create(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));
            ActivityStartDefinitionForTest target = new ActivityStartDefinitionForTest(dummyActivity,
                                                                                       new TimePeriodWithSegment(0, 10, 1, 0, 15),
                                                                                       new TimePeriodWithSegment(16, 50, 19, 0, 15));

            IList<IWorkShift> retColl = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(1, retColl.Count);
            Assert.AreEqual(WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(16, 50, 0), new TimeSpan(17, 0, 0)), retColl[0].LayerCollection[1].Period);
        }

        [Test]
        public void VerifyProcessShiftNotAddAfter()
        {
            WorkShift originalShift = WorkShiftFactory.Create(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));

            ActivityStartDefinitionForTest target = new ActivityStartDefinitionForTest(dummyActivity,
                                                                                       new TimePeriodWithSegment(0, 10, 1, 0, 15),
                                                                                       new TimePeriodWithSegment(16, 51, 19, 0, 15));
            IList<IWorkShift> retColl = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(0, retColl.Count);
        }

        [Test]
        public void VerifyActivityStartsTooEarly()
        {
            WorkShift originalShift = WorkShiftFactory.Create(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));

            IWorkShiftExtender target = new ActivityAbsoluteStartExtender(dummyActivity,
                                                                          new TimePeriodWithSegment(1, 0, 1, 0, 15),
                                                                          new TimePeriodWithSegment(1, 0, 9, 0, 15));
            IList<IWorkShift> retColl = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(1, retColl.Count);
            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0)),
                retColl[0].LayerCollection[1].Period);
        }

        [Test]
        public void VerifyProtectedConstructor()
        {
            Assert.IsTrue(
                ReflectionHelper.HasDefaultConstructor(typeof (ActivityAbsoluteStartExtender)));
        }

        [Test]
        public void CannotExtendBeforeShift()
        {
            WorkShift originalShift = WorkShiftFactory.Create(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0));
            IWorkShiftExtender target = new ActivityAbsoluteStartExtender(dummyActivity,
                                                                        new TimePeriodWithSegment(1, 0, 1, 0, 15),
                                                                        new TimePeriodWithSegment(1, 0, 8, 59, 15)); 
            IList<IWorkShift> retColl = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(0, retColl.Count);
        }
        [Test]
        public void CannotExtendAfterShift()
        {
            WorkShift originalShift = WorkShiftFactory.Create(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0));
            IWorkShiftExtender target = new ActivityAbsoluteStartExtender(dummyActivity,
                                                                        new TimePeriodWithSegment(1, 0, 1, 0, 15),
                                                                        new TimePeriodWithSegment(10, 1, 10, 59, 15));
            IList<IWorkShift> retColl = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(0, retColl.Count);
        }

        internal class ActivityStartDefinitionForTest : ActivityAbsoluteStartExtender
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
    }
}