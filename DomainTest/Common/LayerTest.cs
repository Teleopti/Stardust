using System;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for class Layer
    /// </summary>
    [TestFixture]
    public class LayerTest
    {
        private Activity fakeActivity;

        /// <summary>
        /// Runs before each test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            fakeActivity = ActivityFactory.CreateActivity("dummy",Color.DeepPink);
            fakeActivity.GroupingActivity = new GroupingActivity("test");
        }

        /// <summary>
        /// Can create layer of some sort
        /// </summary>
        [Test]
        public void CanCreateLayerAndPropertiesAreSet()
        {
            DateTimePeriod per =
                new DateTimePeriod(new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2000, 1, 1, 11, 0, 0, DateTimeKind.Utc));
            FakeLayerClass actL = new FakeLayerClass(fakeActivity, per);
            Assert.AreEqual(per, actL.Period);
            Assert.AreSame(fakeActivity, actL.Payload);
            Assert.AreSame(fakeActivity, ((ILayer)actL).Payload);

            FakeLayerClass actL2 = new FakeLayerClass(fakeActivity, per);
            actL2.Payload = actL.Payload;

            Assert.AreEqual(actL.Payload, actL2.Payload);
        }

		[Test, ExpectedException(typeof(NotSupportedException))]
		public void ShouldNotSupportParent()
		{
			var per =
				new DateTimePeriod(new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc),
								   new DateTime(2000, 1, 1, 11, 0, 0, DateTimeKind.Utc));
			var actL = new FakeLayerClass(fakeActivity, per);
			Assert.That(actL.Parent, Is.Null); 
		}

		[Test, ExpectedException(typeof(NotSupportedException))]
		public void ShouldNotSupportSetParent()
		{
			var per =
				new DateTimePeriod(new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc),
								   new DateTime(2000, 1, 1, 11, 0, 0, DateTimeKind.Utc));
			var actL = new FakeLayerClass(fakeActivity, per);
			actL.SetParent(null);
		}
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyProtectedConstructorWorks()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(typeof(ActivityLayer)));
        }


        [Test]
        public void VerifyPayloadSetter()
        {
            DateTimePeriod per =
                new DateTimePeriod(new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2000, 1, 1, 11, 0, 0, DateTimeKind.Utc));
            FakeLayerClass actL = new FakeLayerClass(fakeActivity, per);

            ILayer castedLayer = actL;

            IActivity act = new Activity("sdf");
            castedLayer.Payload = act;
            Assert.AreSame(act, castedLayer.Payload);
        }


        /// <summary>
        /// Activities must not be set to null when creating an activitylayer.
        /// </summary>
        [ExpectedException(typeof (ArgumentNullException))]
        [Test]
        public void PayloadMustNotBeSetToNull()
        {
            new FakeLayerClass(null, new DateTimePeriod());
        }

        /// <summary>
        /// Verifies the adjacent method.
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-01-30
        /// </remarks>
        [Test]
        public void VerifyAdjacent()
        {
            ILayer<IActivity> layer2000 = new ActivityLayer(fakeActivity, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
            ILayer<IActivity> layer2001 = new ActivityLayer(fakeActivity, new DateTimePeriod(2001, 1, 1, 2002, 1, 1));
            ILayer<IActivity> layer2002 = new ActivityLayer(fakeActivity, new DateTimePeriod(2002, 1, 1, 2003, 1, 1));

            Assert.IsTrue(layer2000.AdjacentTo(layer2001));
            Assert.IsTrue(layer2002.AdjacentTo(layer2001));
            Assert.IsFalse(layer2002.AdjacentTo(layer2000));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyNotNullToAdjacent()
        {
            ILayer<IActivity> layer2000 = new ActivityLayer(fakeActivity, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
            layer2000.AdjacentTo(null);
        }

        /// <summary>
        /// Verifies the clone works as expected.
        /// </summary>
        [Test]
        public void VerifyCloneWorksAsExpected()
        {
            var period =
                new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var orgLayer = new FakeLayerClass(fakeActivity, period);

            var cloneLayer = (FakeLayerClass)orgLayer.Clone();
            Assert.AreNotSame(orgLayer, cloneLayer);
            Assert.AreSame(fakeActivity, cloneLayer.Payload);
            Assert.AreEqual(period, cloneLayer.Period);
        }

        [Test]
        public void VerifyICloneableEntity()
        {
            var period =
                new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var orgLayer = new FakeLayerClass(fakeActivity, period);

            var cloneLayer = (FakeLayerClass)orgLayer.EntityClone();
			Assert.That(cloneLayer, Is.Not.Null);
            cloneLayer = (FakeLayerClass)orgLayer.NoneEntityClone();
			Assert.That(cloneLayer, Is.Not.Null);
        }
        /// <summary>
        /// Verifies the order index works.
        /// TODO: more test here when insert, delete and so on exists
        /// </summary>
        [Test]
        public void VerifyOrderIndexWorks()
        {
            var shift = MainShiftFactory.CreateMainShiftWithDefaultCategory();
            var layer1 = new MainShiftActivityLayer(fakeActivity, new DateTimePeriod());
            var layer2 = new MainShiftActivityLayer(fakeActivity, new DateTimePeriod());

            shift.LayerCollection.Add(layer1);
            shift.LayerCollection.Add(layer2);
            Assert.AreEqual(0, layer1.OrderIndex);
            Assert.AreEqual(1, layer2.OrderIndex);
            
            

        }

        /// <summary>
        /// Verifies that a new correct DateTimePeriod (end time) is created on specific layer
        /// </summary>
        [Test]
        public void CanChangePeriodEndTimeAccordingToTimeSpan()
        {
            Activity act = ActivityFactory.CreateActivity("Telefon");
            DateTimePeriod period =
                new DateTimePeriod(new DateTime(2001, 1, 1, 16, 00, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 17, 00, 0, DateTimeKind.Utc));
            ActivityLayer layer = new ActivityLayer(act, period);

            DateTimePeriod expectedPeriod =
                new DateTimePeriod(new DateTime(2001, 1, 1, 16, 00, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 18, 00, 0, DateTimeKind.Utc));
            ActivityLayer expectedLayer = new ActivityLayer(act, expectedPeriod);

            layer.ChangeLayerPeriodEnd(new TimeSpan(0, 1, 0, 0));

            Assert.AreEqual(layer.Period.StartDateTime, expectedLayer.Period.StartDateTime);
            Assert.AreEqual(layer.Period.EndDateTime, expectedLayer.Period.EndDateTime);
        }

        [Test]
        public void CanSetProperties()
        {
            DateTimePeriod period =
                new DateTimePeriod(new DateTime(2001, 1, 1, 16, 00, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 17, 00, 0, DateTimeKind.Utc));
            ActivityLayer layer = new ActivityLayer(fakeActivity, period);
            Activity activity = ActivityFactory.CreateActivity("sdf");
            period= new DateTimePeriod(2000,1,1,2002,1,1);
            layer.Period = period;
            layer.Payload = activity;

            Assert.AreSame(activity, layer.Payload);
            Assert.AreEqual(period, layer.Period);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotSetNullAsPayload()
        {
            DateTimePeriod period =
                new DateTimePeriod(new DateTime(2001, 1, 1, 16, 00, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 17, 00, 0, DateTimeKind.Utc));
            ActivityLayer layer = new ActivityLayer(fakeActivity, period);
            layer.Payload = null;
        }

        /// <summary>
        /// Verifies that a new correct DateTimePeriod (start time) is created on specific layer
        /// </summary>
        [Test]
        public void CanChangePeriodStartTimeAccordingToTimeSpan()
        {
            Activity act = ActivityFactory.CreateActivity("Telefon");
            DateTimePeriod period =
                new DateTimePeriod(new DateTime(2001, 1, 1, 16, 00, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 17, 00, 0, DateTimeKind.Utc));
            ActivityLayer layer = new ActivityLayer(act, period);

            DateTimePeriod expectedPeriod =
                new DateTimePeriod(new DateTime(2001, 1, 1, 15, 00, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 17, 00, 0, DateTimeKind.Utc));
            ActivityLayer expectedLayer = new ActivityLayer(act, expectedPeriod);

            layer.ChangeLayerPeriodStart(new TimeSpan(0, -1, 0, 0));

            Assert.AreEqual(layer.Period.StartDateTime, expectedLayer.Period.StartDateTime);
            Assert.AreEqual(layer.Period.EndDateTime, expectedLayer.Period.EndDateTime);
        }

        /// <summary>
        /// Verifies that the layer is moved forward in time
        /// </summary>
        [Test]
        public void CanMoveLayerForward()
        {
            Activity act = ActivityFactory.CreateActivity("Telefon");
            DateTimePeriod period =
                new DateTimePeriod(new DateTime(2001, 1, 1, 16, 00, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 17, 00, 0, DateTimeKind.Utc));
            ActivityLayer layer = new ActivityLayer(act, period);

            DateTimePeriod expectedPeriod =
                new DateTimePeriod(new DateTime(2001, 1, 1, 18, 00, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 19, 00, 0, DateTimeKind.Utc));
            ActivityLayer expectedLayer = new ActivityLayer(act, expectedPeriod);

            layer.MoveLayer(new TimeSpan(0, 2, 0, 0));

            Assert.AreEqual(expectedLayer.Period.StartDateTime, layer.Period.StartDateTime);
            Assert.AreEqual(expectedLayer.Period.EndDateTime, layer.Period.EndDateTime);
        }

        /// <summary>
        /// Verifies that the layer is moved backwards in time
        /// </summary>
        [Test]
        public void CanMoveLayerBackwards()
        {
            Activity act = ActivityFactory.CreateActivity("Telefon");
            DateTimePeriod period =
                new DateTimePeriod(new DateTime(2001, 1, 1, 16, 00, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 17, 00, 0, DateTimeKind.Utc));
            ActivityLayer layer = new ActivityLayer(act, period);

            DateTimePeriod expectedPeriod =
                new DateTimePeriod(new DateTime(2001, 1, 1, 14, 00, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 15, 00, 0, DateTimeKind.Utc));
            ActivityLayer expectedLayer = new ActivityLayer(act, expectedPeriod);

            layer.MoveLayer(new TimeSpan(0, -2, 0, 0));

            Assert.AreEqual(expectedLayer.Period.StartDateTime, layer.Period.StartDateTime);
            Assert.AreEqual(expectedLayer.Period.EndDateTime, layer.Period.EndDateTime);
        }


        /// <summary>
        /// Verify that transform works
        /// </summary>
        [Test]
        public void CanTransformOneLayerToAnother()
        {
            DateTimePeriod per = new DateTimePeriod(new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc), 
                                    new DateTime(2000, 1, 1, 11, 0, 0, DateTimeKind.Utc));

            DateTimePeriod newPer = new DateTimePeriod(new DateTime(2000, 1, 5, 10, 0, 0, DateTimeKind.Utc),
                                    new DateTime(2000, 1, 5, 12, 0, 0, DateTimeKind.Utc));

            FakeLayerClass actL = new FakeLayerClass(fakeActivity, per);

            Activity activity = new Activity("Kalle");

            FakeLayerClass newActL = new FakeLayerClass(activity, newPer);


            actL.Transform(newActL);

            Assert.AreEqual("Kalle", actL.Payload.Description.Name);
            Assert.AreEqual(newPer, actL.Period);

        }

        private class FakeLayerClass : Layer<IActivity>
        {
            public FakeLayerClass(IActivity act, DateTimePeriod period) : base(act, period)
            {
            }

        }
    }
}