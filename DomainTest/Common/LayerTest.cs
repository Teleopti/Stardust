using System;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for class Layer
    /// </summary>
    [TestFixture]
    public class LayerTest
    {
        private IActivity fakeActivity;

        /// <summary>
        /// Runs before each test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            fakeActivity = ActivityFactory.CreateActivity("dummy",Color.DeepPink);
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

            FakeLayerClass actL2 = new FakeLayerClass(fakeActivity, per);

            Assert.AreEqual(actL.Payload, actL2.Payload);
        }

        /// <summary>
        /// Activities must not be set to null when creating an activitylayer.
        /// </summary>
        [Test]
        public void PayloadMustNotBeSetToNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FakeLayerClass(null, new DateTimePeriod()));
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

        [Test]
        public void CanSetProperties()
        {
            DateTimePeriod period =
                new DateTimePeriod(new DateTime(2001, 1, 1, 16, 00, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 17, 00, 0, DateTimeKind.Utc));
            ActivityLayer layer = new ActivityLayer(fakeActivity, period);
            period= new DateTimePeriod(2000,1,1,2002,1,1);
            layer.Period = period;
            Assert.AreEqual(period, layer.Period);
        }

       

        private class FakeLayerClass : Layer<IActivity>
        {
            public FakeLayerClass(IActivity act, DateTimePeriod period) : base(act, period)
            {
            }

        }
    }
}