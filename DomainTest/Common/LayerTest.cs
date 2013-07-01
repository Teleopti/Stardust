using System;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
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

            FakeLayerClass actL2 = new FakeLayerClass(fakeActivity, per);

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

						var defSet = new MultiplicatorDefinitionSet("d", MultiplicatorType.Overtime);
						var shift = OvertimeShiftFactory.CreateOvertimeShift(new Activity("d"),
																																			new DateTimePeriod(2000, 1, 1, 2000, 1, 2),
																																			defSet,
																																			new PersonAssignment(new Person(), new Scenario("d"),
																																													 new DateOnly(2000, 1, 1)));
						var layer2 = new OvertimeShiftActivityLayer(ActivityFactory.CreateActivity("hej"), new DateTimePeriod(2000, 1, 1, 2002, 1, 1), defSet);
						shift.LayerCollection.Add(layer2);
						Assert.AreEqual(1, layer2.OrderIndex);
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