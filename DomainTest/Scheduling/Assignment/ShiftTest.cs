using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    /// <summary>
    /// Test for shift
    /// </summary>
    [TestFixture]
    public class ShiftTest
    {
        private MainShift source;

		private MainShift destination1;
		private MainShift destination2;

        /// <summary>
        /// Run once for every test.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            source = new FakeShift();
            destination1 = new FakeShift();
            destination2 = new FakeShift();
        }

        /// <summary>
        /// Verify shift transformation
        /// </summary>
        [Test]
        public void CanTransformShiftToDestinationWithFewerLayers()
        {
            CreateSource();
            CreateDestination2();

            destination2.Transform(source);

            Assert.AreEqual(2, destination2.LayerCollection.Count);
            Assert.AreEqual(source.LayerCollection[0].Payload.Description.Name, destination2.LayerCollection[0].Payload.Description.Name);
            Assert.AreEqual(source.LayerCollection[0].Period.EndDateTime, destination2.LayerCollection[0].Period.EndDateTime);
        }

        /// <summary>
        /// Verify shift transformation 
        /// </summary>
        [Test]
        public void CanTransformShiftToDestinationWithMoreLayers()
        {
            CreateSource();
            CreateDestination1();

            destination1.Transform(source);

            Assert.AreEqual(2, destination1.LayerCollection.Count);
            Assert.AreEqual(source.LayerCollection[0].Payload.Description.Name, destination1.LayerCollection[0].Payload.Description.Name);
            Assert.AreEqual(source.LayerCollection[0].Period.EndDateTime, destination1.LayerCollection[0].Period.EndDateTime);
	        destination1.ShiftCategory.Should().Be.SameInstanceAs(source.ShiftCategory);
        }

        /// <summary>
        /// CreateProjection shift to transform from
        /// </summary>
        private void CreateSource()
        {
            DateTimePeriod period1 =
                new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            DateTimePeriod period2 =
                new DateTimePeriod(new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2003, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            typeof(Entity).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(source, Guid.NewGuid());

            source.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("hej1"), period1));

			source.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("hej2"), period2));
					source.ShiftCategory = new ShiftCategory("test");
        }

        /// <summary>
        /// CreateProjection shift to transform to, 3 periods in this shift
        /// </summary>
        private void CreateDestination1()
        {
            DateTimePeriod period1 =
                new DateTimePeriod(new DateTime(2000, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc));

            DateTimePeriod period2 =
                new DateTimePeriod(new DateTime(2002, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2003, 1, 2, 0, 0, 0, DateTimeKind.Utc));

            DateTimePeriod period3 =
                new DateTimePeriod(new DateTime(2002, 1, 3, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2003, 1, 3, 0, 0, 0, DateTimeKind.Utc));

            typeof(Entity).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(destination1, Guid.NewGuid());

			destination1.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("bye1"), period1));

			destination1.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("bye2"), period2));

			destination1.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("bye3"), period3));
        }

        /// <summary>
        /// CreateProjection shift to transform to, 1 period in this shift
        /// </summary>
        private void CreateDestination2()
        {
            DateTimePeriod period1 =
                new DateTimePeriod(new DateTime(2000, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc));


            typeof(Entity).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(destination2, Guid.NewGuid());

            destination2.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("bye1"), period1));
        }

        [Test]
        public void VerifyHasProjectionTrue()
        {
			source.LayerCollection.Add(new MainShiftActivityLayer(new Activity("sdf"), new DateTimePeriod(2002, 1, 1, 2003, 1, 1)));
            Assert.IsTrue(source.HasProjection);
        }

        [Test]
        public void VerifyHasProjectionFalse()
        {
            Assert.IsFalse(source.HasProjection);
        }

        [Test]
        public void VerifyProjectionWithZeroItems()
        {
            IProjectionService svc = source.ProjectionService();

            Assert.IsNull(svc.CreateProjection().Period());
        }

        [Test]
        public void VerifyProjectionWithOneItem()
        {
            ActivityLayer actLayer =
				new MainShiftActivityLayer(new Activity("sdfsdf"), new DateTimePeriod(2000, 1, 1, 2002, 1, 1));
            source.LayerCollection.Add(actLayer);
            IProjectionService svc = source.ProjectionService();

            IVisualLayerCollection retList = svc.CreateProjection();
            Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2002, 1, 1), retList.Period());
        }

        [Test]
        public void VerifyProjectionWithMultipleItems()
        {
            Activity act1 = new Activity("1");
            Activity act2 = new Activity("2");

            source.LayerCollection.Add(
				new MainShiftActivityLayer(act1, new DateTimePeriod(2000, 1, 1, 2005, 1, 1)));
            source.LayerCollection.Add(
				new MainShiftActivityLayer(act2, new DateTimePeriod(2001, 1, 1, 2002, 1, 1)));
            source.LayerCollection.Add(
				new MainShiftActivityLayer(act2, new DateTimePeriod(2003, 1, 1, 2004, 1, 1)));

            IProjectionService svc = source.ProjectionService();
            IList<IVisualLayer> resWrapper = new List<IVisualLayer>(svc.CreateProjection());

            Assert.AreEqual(5, resWrapper.Count);
            Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2001, 1, 1), resWrapper[0].Period);
            Assert.AreSame(act1, resWrapper[0].Payload);
            Assert.AreEqual(new DateTimePeriod(2001, 1, 1, 2002, 1, 1), resWrapper[1].Period);
            Assert.AreSame(act2, resWrapper[1].Payload);
            Assert.AreEqual(new DateTimePeriod(2002, 1, 1, 2003, 1, 1), resWrapper[2].Period);
            Assert.AreSame(act1, resWrapper[2].Payload);
            Assert.AreEqual(new DateTimePeriod(2003, 1, 1, 2004, 1, 1), resWrapper[3].Period);
            Assert.AreSame(act2, resWrapper[3].Payload);
            Assert.AreEqual(new DateTimePeriod(2004, 1, 1, 2005, 1, 1), resWrapper[4].Period);
            Assert.AreSame(act1, resWrapper[4].Payload);
        }



    }

    /// <summary>
    /// Class to fake an implementation of shift
    /// </summary>
	internal class FakeShift : MainShift
    {

    }
}