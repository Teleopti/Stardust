using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
    [TestFixture]
    public class WorkShiftTemplateGeneratorTest
    {
        private Activity dummyActivity;
        private ShiftCategory category;

        [SetUp]
        public void Setup()
        {
            dummyActivity = new Activity("for test");
            category = new ShiftCategory("cat for test");
        }


        [Test]
        public void VerifyProtectedConstructor()
        {
            Assert.IsTrue(
                ReflectionHelper.HasDefaultConstructor(typeof(WorkShiftTemplateGenerator)));
        }

        [Test]
        public void VerifyActivityIsNotNull()
        {
			Assert.Throws<ArgumentNullException>(() => new WorkShiftTemplateGenerator(null, new TimePeriodWithSegment(8, 0, 9, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15), category));
        }

        [Test]
        public void VerifyCategoryIsNotNull()
        {
			Assert.Throws<ArgumentNullException>(() => new WorkShiftTemplateGenerator(dummyActivity, new TimePeriodWithSegment(8, 0, 9, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15), null));
        }


        [Test]
        public void VerifyCannotSetActivityToNull()
        {
            IWorkShiftTemplateGenerator template = new WorkShiftTemplateGenerator(dummyActivity,
                                                                                  new TimePeriodWithSegment(8, 0, 9, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15), category);
			Assert.Throws<ArgumentNullException>(() => template.BaseActivity = null);
        }

        [Test]
        public void VerifyCannotSetCategoryToNull()
        {
            IWorkShiftTemplateGenerator template = new WorkShiftTemplateGenerator(dummyActivity,
                                                                                  new TimePeriodWithSegment(8, 0, 9, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15), category);
			Assert.Throws<ArgumentNullException>(() => template.Category = null);
        }

        [Test]
        public void VerifyCanGetAndSetProperties()
        {
            Activity newAct = new Activity("sdf");
            TimePeriodWithSegment startPer = new TimePeriodWithSegment(8, 0, 11, 0, 15);
            TimePeriodWithSegment endPer = new TimePeriodWithSegment(13, 0, 16, 0, 15);
            category = new ShiftCategory("sdf");

            IWorkShiftTemplateGenerator template = new WorkShiftTemplateGenerator(newAct,
                                                                                  new TimePeriodWithSegment(8, 0, 9, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15), category);
            template.BaseActivity = newAct;
            template.StartPeriod = startPer;
            template.EndPeriod = endPer;
            template.Category = category;

            Assert.AreSame(newAct, template.BaseActivity);
            Assert.AreEqual(startPer, template.StartPeriod);
            Assert.AreEqual(endPer, template.EndPeriod);
            Assert.AreSame(category, template.Category);
        }


        [Test]
        public void VerifyCreateMainShiftNotOutsideLimits()
        {
            IList<IWorkShift> theList = new WorkShiftTemplateGenerator(dummyActivity,
                                                                       new TimePeriodWithSegment(8, 0, 9, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15), category).Generate();

            Assert.AreEqual(5, theList.Count);
            assertPeriod(theList, new TimeSpan(8, 0, 0), new TimeSpan(15, 0, 0));
            assertPeriod(theList, new TimeSpan(8, 15, 0), new TimeSpan(15, 0, 0));
            assertPeriod(theList, new TimeSpan(8, 30, 0), new TimeSpan(15, 0, 0));
            assertPeriod(theList, new TimeSpan(8, 45, 0), new TimeSpan(15, 0, 0));
            assertPeriod(theList, new TimeSpan(9, 0, 0), new TimeSpan(15, 0, 0));
        }

        [Test]
        public void VerifyCreateMainShiftNotOutsideLimits2()
        {
            IList<IWorkShift> theList = new WorkShiftTemplateGenerator(dummyActivity,
                                                                       new TimePeriodWithSegment(8, 0, 9, 0, 15), new TimePeriodWithSegment(15, 0, 16, 0, 15), category).Generate();

            Assert.AreEqual(25, theList.Count);
            assertPeriod(theList, new TimeSpan(8, 0, 0), new TimeSpan(15, 0, 0));
            assertPeriod(theList, new TimeSpan(8, 15, 0), new TimeSpan(15, 0, 0));
            assertPeriod(theList, new TimeSpan(8, 30, 0), new TimeSpan(15, 0, 0));
            assertPeriod(theList, new TimeSpan(8, 45, 0), new TimeSpan(15, 0, 0));
            assertPeriod(theList, new TimeSpan(9, 0, 0), new TimeSpan(15, 0, 0));
        }

        [Test]
        public void VerifyCreateMainShiftNotOutsideLimits3()
        {
            IList<IWorkShift> theList = new WorkShiftTemplateGenerator(dummyActivity,
                                                                       new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(15, 0, 16, 0, 5), category).Generate();

            Assert.AreEqual(13, theList.Count);

            assertPeriod(theList, new TimeSpan(8, 0, 0), new TimeSpan(15, 0, 0));
            assertPeriod(theList, new TimeSpan(8, 0, 0), new TimeSpan(15, 15, 0));
            assertPeriod(theList, new TimeSpan(8, 0, 0), new TimeSpan(15, 30, 0));
            assertPeriod(theList, new TimeSpan(8, 0, 0), new TimeSpan(15, 55, 0));
            assertPeriod(theList, new TimeSpan(8, 0, 0), new TimeSpan(16, 0, 0));
        }

        [Test]
        public void VerifyCreateMainShiftNotOutsideLimits4()
        {
            IList<IWorkShift> theList = new WorkShiftTemplateGenerator(dummyActivity,
                                                                       new TimePeriodWithSegment(7, 0, 10, 0, 5), new TimePeriodWithSegment(14, 0, 18, 0, 5), category).Generate();

            Assert.AreEqual(1813, theList.Count);
            assertPeriod(theList, new TimeSpan(8, 0, 0), new TimeSpan(15, 0, 0));
            assertPeriod(theList, new TimeSpan(8, 0, 0), new TimeSpan(15, 15, 0));
            assertPeriod(theList, new TimeSpan(8, 0, 0), new TimeSpan(15, 30, 0));
            assertPeriod(theList, new TimeSpan(8, 0, 0), new TimeSpan(15, 55, 0));
            assertPeriod(theList, new TimeSpan(8, 0, 0), new TimeSpan(16, 0, 0));
        }

        [Test]
        public void VerifyCreateMainShiftOverMidnight()
        {
            IList<IWorkShift> theList = new WorkShiftTemplateGenerator(dummyActivity,
                                                                       new TimePeriodWithSegment(23, 0, 23, 0, 15), new TimePeriodWithSegment(26, 0, 27, 0, 15), category).Generate();

            Assert.AreEqual(5, theList.Count);
            assertPeriod(theList, new TimeSpan(23, 0, 0), new TimeSpan(1, 2, 0, 0));
            assertPeriod(theList, new TimeSpan(23, 0, 0), new TimeSpan(1, 2, 15, 0));
            assertPeriod(theList, new TimeSpan(23, 0, 0), new TimeSpan(1, 2, 30, 0));
            assertPeriod(theList, new TimeSpan(23, 0, 0), new TimeSpan(1, 2, 45, 0));
            assertPeriod(theList, new TimeSpan(23, 0, 0), new TimeSpan(1, 3, 0, 0));
        }

        [Test]
        public void VerifyCreateMainShiftOverMidnight2()
        {
            IList<IWorkShift> theList = new WorkShiftTemplateGenerator(dummyActivity,
                                                                       new TimePeriodWithSegment(23, 0, 25, 0, 15), new TimePeriodWithSegment(30, 0, 30, 0, 15), category).Generate();

            Assert.AreEqual(9, theList.Count);
            assertPeriod(theList, new TimeSpan(23, 0, 0), new TimeSpan(1, 6, 0, 0));
            assertPeriod(theList, new TimeSpan(23, 15, 0), new TimeSpan(1, 6, 0, 0));
            assertPeriod(theList, new TimeSpan(23, 45, 0), new TimeSpan(1, 6, 0, 0));
            assertPeriod(theList, new TimeSpan(24, 0, 0), new TimeSpan(1, 6, 0, 0));
            assertPeriod(theList, new TimeSpan(25, 0, 0), new TimeSpan(1, 6, 0, 0));
        }

        [Test]
        public void VerifyCreateMainShiftOverMidnight3()
        {
            IList<IWorkShift> theList = new WorkShiftTemplateGenerator(dummyActivity,
                                                                       new TimePeriodWithSegment(18, 0, 18, 0, 15), new TimePeriodWithSegment(23, 0, 25, 0, 15), category).Generate();

            Assert.AreEqual(9, theList.Count);
            assertPeriod(theList, new TimeSpan(18, 0, 0), new TimeSpan(23, 0, 0));
            assertPeriod(theList, new TimeSpan(18, 0, 0), new TimeSpan(23, 30, 0));
            assertPeriod(theList, new TimeSpan(18, 0, 0), new TimeSpan(24, 0, 0));
            assertPeriod(theList, new TimeSpan(18, 0, 0), new TimeSpan(24, 30, 0));
            assertPeriod(theList, new TimeSpan(18, 0, 0), new TimeSpan(25, 0, 0));
        }

        [Test]
        public void VerifyCreateMainShiftOverMidnight4()
        {
            IList<IWorkShift> theList = new WorkShiftTemplateGenerator(dummyActivity,
                                                                       new TimePeriodWithSegment(18, 0, 18, 0, 15), new TimePeriodWithSegment(25, 0, 26, 0, 15), category).Generate();

            Assert.AreEqual(5, theList.Count);
            assertPeriod(theList, new TimeSpan(18, 0, 0), new TimeSpan(25, 0, 0));
            assertPeriod(theList, new TimeSpan(18, 0, 0), new TimeSpan(25, 15, 0));
            assertPeriod(theList, new TimeSpan(18, 0, 0), new TimeSpan(25, 30, 0));
            assertPeriod(theList, new TimeSpan(18, 0, 0), new TimeSpan(25, 45, 0));
            assertPeriod(theList, new TimeSpan(18, 0, 0), new TimeSpan(26, 0, 0));
        }

        [Test]
        public void VerifyEndPeriodStartsBeforeStartPeriodEnds()
        {
            IList<IWorkShift> theList = new WorkShiftTemplateGenerator(dummyActivity,
                                                                       new TimePeriodWithSegment(10, 0, 12, 0, 60), new TimePeriodWithSegment(11, 0, 13, 0, 60), category)
                                                                       .Generate();
            Assert.AreEqual(6, theList.Count);
            assertPeriod(theList, new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0));
            assertPeriod(theList, new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0));
            assertPeriod(theList, new TimeSpan(10, 0, 0), new TimeSpan(13, 0, 0));
            assertPeriod(theList, new TimeSpan(11, 0, 0), new TimeSpan(12, 0, 0));
            assertPeriod(theList, new TimeSpan(11, 0, 0), new TimeSpan(13, 0, 0));
            assertPeriod(theList, new TimeSpan(12, 0, 0), new TimeSpan(13, 0, 0));
        }

        private void assertPeriod(IEnumerable<IWorkShift> shiftColl,
                                         TimeSpan start,
                                         TimeSpan end)
        {
            bool existFlag = false;
            foreach (IWorkShift shift in shiftColl)
            {
                if (shift.LayerCollection[0].Period.Equals(new DateTimePeriod(WorkShift.BaseDate.Add(start),
                                                                                    WorkShift.BaseDate.Add(end))))
                {
                    Assert.AreSame(category, shift.ShiftCategory);
                    existFlag = true;
                    break;
                }
            }
            Assert.IsTrue(existFlag);
        }

        [Test]
        public void VerifyICloneableEntity()
        {
            IWorkShiftTemplateGenerator target = new WorkShiftTemplateGenerator(dummyActivity,
                                                           new TimePeriodWithSegment(10, 0, 12, 0, 60),
                                                           new TimePeriodWithSegment(11, 0, 13, 0, 60),
                                                           category);

            IWorkShiftTemplateGenerator targetCloned = (IWorkShiftTemplateGenerator)target.Clone();
            Assert.IsNotNull(targetCloned);

            targetCloned.BaseActivity = new Activity("some other activity");
            Assert.AreNotSame(target.BaseActivity, targetCloned.BaseActivity);
        }
    }
}