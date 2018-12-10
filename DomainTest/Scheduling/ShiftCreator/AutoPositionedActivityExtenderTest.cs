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
    public class AutoPositionedActivityExtenderTest
    {
        private Activity activity;
        private AutoPositionedActivityExtender target;

        [SetUp]
        public void Setup()
        {
            activity = new Activity("Activity to be placed");
        }


        [Test]
        public void VerifyProtectedConstructor()
        {
            Assert.IsTrue(
                ReflectionHelper.HasDefaultConstructor(typeof(AutoPositionedActivityExtender)));
        }

        [Test]
        public void VerifyActivityIsNotNull()
        {
			Assert.Throws<ArgumentNullException>(() => target = new AutoPositionedActivityExtender(null, new TimePeriodWithSegment(), new TimeSpan()));
        }

        [Test]
        public void VerifyStartSegmentHasPositiveValue()
        {
			Assert.Throws<ArgumentOutOfRangeException>(() => target = new AutoPositionedActivityExtender(activity, new TimePeriodWithSegment(), new TimeSpan(0)));
        }

        [Test]
        public void VerifyStartSegmentPropertyHasPositiveValue()
        {
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			target = new AutoPositionedActivityExtender(activity, new TimePeriodWithSegment(), new TimeSpan(1))
	        {
		        StartSegment = new TimeSpan(0)
	        });
        }

        [Test]
        public void VerifyDefaultProperties()
        {
            target = new AutoPositionedActivityExtender(activity, new TimePeriodWithSegment(0, 10, 0, 20, 15), new TimeSpan(10));
            Assert.AreSame(activity, target.ExtendWithActivity);
            Assert.AreEqual(new TimeSpan(0, 20, 0), target.ExtendMaximum());
            Assert.AreEqual(new TimePeriodWithSegment(0, 10, 0, 20, 15), target.ActivityLengthWithSegment);
            Assert.AreEqual(new TimeSpan(10), target.StartSegment);
            Assert.AreEqual(1, target.NumberOfLayers);
        }

        [Test]
        public void VerifyCanSetProperties()
        {
            TimeSpan newSeg = new TimeSpan(24);
            byte newNoOf = 37;
            TimeSpan newAuto = new TimeSpan(24);
            target = new AutoPositionedActivityExtender(activity, new TimePeriodWithSegment(0, 10, 0, 20, 15), new TimeSpan(10));
            target.NumberOfLayers = newNoOf;
            target.AutoPositionIntervalSegment = newAuto;
            target.StartSegment = newSeg;
            Assert.AreEqual(newNoOf, target.NumberOfLayers);
            Assert.AreEqual(newAuto, target.AutoPositionIntervalSegment);
            Assert.AreEqual(newSeg, target.StartSegment);
        }

        [Test]
        public void VerifyTwoLayerWithOneLengthOnSimpleShift()
        {
            IWorkShift originalShift = WorkShiftFactory.CreateWithLunch(new TimePeriod(9, 0, 16, 0), new TimePeriod(11,0,12,0));

            TimePeriodWithSegment activityLength = new TimePeriodWithSegment(1, 0, 1, 0, 30);
            target = new AutoPositionedActivityExtender(activity, activityLength, new TimeSpan(0, 30, 0), 2);
            IList<IWorkShift> ret = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(1, ret.Count);
            IList<IVisualLayer> proj = new List<IVisualLayer>(ret[0].ProjectionService().CreateProjection());
            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(9, 30, 0), new TimeSpan(10, 30, 0)),
                proj[1].Period
                );
            Assert.AreSame(activity, proj[1].Payload);
            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(13, 30, 0), new TimeSpan(14, 30, 0)),
                proj[5].Period
                );
            Assert.AreSame(activity, proj[5].Payload);

        }

        [Test]
        public void VerifyDoNothingIfNumberOfLayersIsZero()
        {
            IWorkShift originalShift = WorkShiftFactory.CreateWithLunch(new TimePeriod(9, 0, 16, 0), new TimePeriod(11, 0, 12, 0));
            TimePeriodWithSegment activityLength = new TimePeriodWithSegment(1, 0, 1, 0, 30);
            target = new AutoPositionedActivityExtender(activity, activityLength, new TimeSpan(0, 30, 0));
            target.NumberOfLayers = 0;
            IList<IWorkShift> ret = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(1, ret.Count);
            Assert.AreSame(originalShift, ret[0]);
        }

        [Test]
        public void VerifyTwoLayersWithTwoLengthsOnEmptyShift()
        {
            WorkShift originalShift = WorkShiftFactory.Create(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));

            TimePeriodWithSegment activityLength = new TimePeriodWithSegment(1,0,1,45,30);

            target = new AutoPositionedActivityExtender(activity, activityLength, new TimeSpan(0, 15, 0), 2);
            IList<IWorkShift> ret = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(2, ret.Count);

            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(11, 0, 0), new TimeSpan(12, 0, 0)),
                ret[0].LayerCollection[1].Period
                );
            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(14, 0, 0), new TimeSpan(15, 0, 0)),
                ret[0].LayerCollection[2].Period
                );
            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(10, 45, 0), new TimeSpan(12, 15, 0)),
                ret[1].LayerCollection[1].Period
                );
            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(13, 45, 0), new TimeSpan(15, 15, 0)),
                ret[1].LayerCollection[2].Period
                );
        }

        [Test]
        public void VerifyCorrectStartTimeWhenSlidingEarlier()
        {
            WorkShift originalShift = WorkShiftFactory.Create(new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0));
            TimePeriodWithSegment activityLength = new TimePeriodWithSegment(0, 20, 0, 20, 30);
            target = new AutoPositionedActivityExtender(activity, activityLength, new TimeSpan(0, 15, 0), 1);
            IList<IWorkShift> ret = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(9, 45, 0), new TimeSpan(10, 5, 0)),
                ret[0].LayerCollection[1].Period
                );
        }

        [Test]
        public void VerifyCorrectStartTimeWhenSlidingLater()
        {
            WorkShift originalShift = WorkShiftFactory.Create(new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0));
            TimePeriodWithSegment activityLength = new TimePeriodWithSegment(0, 10, 0, 10, 30);
            target = new AutoPositionedActivityExtender(activity, activityLength, new TimeSpan(0, 15, 0), 1);
            IList<IWorkShift> ret = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(10, 0, 0), new TimeSpan(10, 10, 0)),
                ret[0].LayerCollection[1].Period
                );
        }

        [Test]
        public void VerifyImpossibleLayer()
        {
            WorkShift originalShift = WorkShiftFactory.Create(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0));
            TimePeriodWithSegment activityLength = new TimePeriodWithSegment(1, 0, 5, 0, 30);
            target = new AutoPositionedActivityExtender(activity, activityLength, new TimeSpan(0, 15, 0), 1);
            IList<IWorkShift> ret = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0)),
                ret[0].LayerCollection[1].Period
                );
        }

        #region TimeSegment tests

        /* 
         * Optimal positions
         * 5min
         * 16:40-21:40
         * 38:20-43:20
         * 10min
         * 13:20-23:20
         * 36:40-46:40
         */
        
        [Test]
        public void VerifyLayersArePlacedCorrectlyWhenTimeSegmentIs1()
        {
            WorkShift originalShift = WorkShiftFactory.Create(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0));
            TimePeriodWithSegment activityLength = new TimePeriodWithSegment(0, 5, 0, 10, 5);
            target = new AutoPositionedActivityExtender(activity, activityLength, new TimeSpan(0, 1, 0), 2);
            IList<IWorkShift> ret = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(2, ret.Count);
            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(9, 17, 0), new TimeSpan(9, 22, 0)),
                ret[0].LayerCollection[1].Period
                );
            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(9, 38, 0), new TimeSpan(9, 43, 0)),
                ret[0].LayerCollection[2].Period
                );
            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(9, 13, 0), new TimeSpan(9, 23, 0)),
                ret[1].LayerCollection[1].Period
                );
            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(9, 37, 0), new TimeSpan(9, 47, 0)),
                ret[1].LayerCollection[2].Period
                );
        }

        [Test]
        public void VerifyLayersArePlacedCorrectlyWhenTimeSegmentIs5()
        {
            WorkShift originalShift = WorkShiftFactory.Create(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0));
            TimePeriodWithSegment activityLength = new TimePeriodWithSegment(0, 5, 0, 10, 5);
            target = new AutoPositionedActivityExtender(activity, activityLength, new TimeSpan(0, 5, 0), 2);
            IList<IWorkShift> ret = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(2, ret.Count);
            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(9, 15, 0), new TimeSpan(9, 20, 0)),
                ret[0].LayerCollection[1].Period
                );
            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(9, 40, 0), new TimeSpan(9, 45, 0)),
                ret[0].LayerCollection[2].Period
                );
            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(9, 15, 0), new TimeSpan(9, 25, 0)),
                ret[1].LayerCollection[1].Period
                );
            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(9, 35, 0), new TimeSpan(9, 45, 0)),
                ret[1].LayerCollection[2].Period
                );
        }

        [Test]
        public void VerifyLayersArePlacedCorrectlyWhenTimeSegmentIs15()
        {
            WorkShift originalShift = WorkShiftFactory.Create(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0));
            TimePeriodWithSegment activityLength = new TimePeriodWithSegment(0, 5, 0, 10, 5);
            target = new AutoPositionedActivityExtender(activity, activityLength, new TimeSpan(0, 15, 0), 2);
            IList<IWorkShift> ret = target.ReplaceWithNewShifts(originalShift);
            Assert.AreEqual(2, ret.Count);
            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(9, 15, 0), new TimeSpan(9, 20, 0)),
                ret[0].LayerCollection[1].Period
                );
            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(9, 45, 0), new TimeSpan(9, 50, 0)),
                ret[0].LayerCollection[2].Period
                );
            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(9, 15, 0), new TimeSpan(9, 25, 0)),
                ret[1].LayerCollection[1].Period
                );
            Assert.AreEqual(
                WorkShiftFactory.DateTimePeriodForWorkShift(new TimeSpan(9, 30, 0), new TimeSpan(9, 40, 0)),
                ret[1].LayerCollection[2].Period
                );
        }
        #endregion


    }
}