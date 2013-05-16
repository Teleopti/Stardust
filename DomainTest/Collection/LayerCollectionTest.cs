using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Collection
{
    /// <summary>
    /// Tests for LayerCollection
    /// </summary>
    [TestFixture]
    public class LayerCollectionTest
    {
        private LayerCollection<IActivity> target;

        [SetUp]
        public void Setup()
        {
            target = new LayerCollection<IActivity>();
        }

        /// <summary>
        /// Verifies the parent works.
        /// </summary>
        [Test]
        public void VerifyParentWorksWhenLayerIsAdded()
        {
	        var dummyShift = PersonalShiftFactory.CreatePersonalShift(ActivityFactory.CreateActivity("hopp"),
	                                                                  new DateTimePeriod(2000, 1, 1, 2002, 1, 1));
            var actLay =
                new PersonalShiftActivityLayer(ActivityFactory.CreateActivity("hej"),
                                                        new DateTimePeriod(2000, 1, 1, 2002, 1, 1));
            dummyShift.LayerCollection.Add(actLay);
            Assert.AreSame(dummyShift, actLay.Parent);
        }

        /// <summary>
        /// Verifies that null cannot be added to list.
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void VerifyNullCannotBeAddedToList()
        {
            target.Add(null);
        }

        /// <summary>
        /// Verifies move period works.
        /// </summary>
        [Test]
        public void VerifyMovePeriod()
        {
            TimeSpan moveTime = new TimeSpan(1, 1, 0);
            Activity act = ActivityFactory.CreateActivity("hejhej");
            DateTimePeriod period1before = new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                                              new DateTime(2001, 1, 1, 1, 1, 0, DateTimeKind.Utc));
            DateTimePeriod period2before = new DateTimePeriod(new DateTime(2000, 1, 1, 4, 0, 0, DateTimeKind.Utc),
                                                              new DateTime(2001, 1, 4, 1, 1, 0, DateTimeKind.Utc));
            DateTimePeriod period1after = period1before.MovePeriod(moveTime);
            DateTimePeriod period2after = period2before.MovePeriod(moveTime);
            ActivityLayer actLay1 = new ActivityLayer(act, period1before);
            ActivityLayer actLay2 = new ActivityLayer(act, period2before);

            target.Add(actLay1);
            target.Add(actLay2);
            target.MoveAllLayers(moveTime);

            Assert.AreEqual(period1after, target[0].Period);
            Assert.AreEqual(period2after, target[1].Period);
        }

        /// <summary>
        /// Verifies that the earliest start time and the latest end time of the layers are returned.
        /// </summary>
        [Test]
        public void VerifyPeriod()
        {
            DateTimePeriod period1 =
                new DateTimePeriod(new DateTime(2007, 8, 10, 11, 15, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 8, 10, 17, 15, 0, DateTimeKind.Utc));
            DateTimePeriod period2 =
                new DateTimePeriod(new DateTime(2007, 8, 10, 11, 15, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 8, 11, 17, 15, 0, DateTimeKind.Utc));
            DateTimePeriod period3 =
                new DateTimePeriod(new DateTime(2007, 8, 10, 12, 15, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 8, 10, 15, 15, 0, DateTimeKind.Utc));
            Activity act = ActivityFactory.CreateActivity("hejhej");
            target.Add(new ActivityLayer(act, period1));
            target.Add(new ActivityLayer(act, period2));
            target.Add(new ActivityLayer(act, period3));
            DateTimePeriod expectedPeriod =
                new DateTimePeriod(new DateTime(2007, 8, 10, 11, 15, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 8, 11, 17, 15, 0, DateTimeKind.Utc));
            Assert.AreEqual(expectedPeriod, target.Period());
        }

        /// <summary>
        /// Verifies the period when list is empty.
        /// </summary>
        [Test]
        public void VerifyPeriodWhenListIsEmpty()
        {
            Assert.IsNull(target.Period());
        }

        /// <summary>
        /// Verifies that valid type could be added.
        /// </summary>
        [Test]
        public void VerifyValidTypeCouldBeAdded()
        {
            MainShift mainShift = MainShiftFactory.CreateMainShiftWithDefaultCategory();
            mainShift.LayerCollection.Add(
                new MainShiftActivityLayer(ActivityFactory.CreateActivity("hj"),
                                                        new DateTimePeriod()));
        }

        /// <summary>
        /// Verifies that valid type could be removed.
        /// </summary>
        [Test]
        public void VerifyValidTypeCouldBeRemoved()
        {
            MainShiftActivityLayer newMainShift =
                new MainShiftActivityLayer(ActivityFactory.CreateActivity("hj"), new DateTimePeriod());

            MainShift mainShift = MainShiftFactory.CreateMainShiftWithDefaultCategory();
            mainShift.LayerCollection.Add(newMainShift);

            mainShift.LayerCollection.Remove(newMainShift);
        }

        /// <summary>
        /// Determines whether this instance [can clear layer collection].
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2007-10-23
        /// </remarks>
        [Test]
        public void CanClearLayerCollection()
        {
            MainShift mainShift = MainShiftFactory.CreateMainShiftWithDefaultCategory();
            mainShift.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("hj"), new DateTimePeriod()));
            mainShift.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("er"), new DateTimePeriod()));

            Assert.AreEqual(2, mainShift.LayerCollection.Count);
            mainShift.LayerCollection.Clear();
            Assert.AreEqual(0,mainShift.LayerCollection.Count);
        }

        [Test]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void CanAddLayerAtIndexedPosition()
        {
            MainShift mainShift = MainShiftFactory.CreateMainShiftWithDefaultCategory();
            mainShift.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("hj"), new DateTimePeriod()));
            mainShift.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("er"), new DateTimePeriod()));

            MainShiftActivityLayer layer = new MainShiftActivityLayer(ActivityFactory.CreateActivity("index0"), new DateTimePeriod());

            mainShift.LayerCollection.Insert(0, layer);
            Assert.AreEqual(mainShift.LayerCollection[0],layer);
        }

        [Test]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void CanMoveUpLayer()
        {
            MainShift mainShift = MainShiftFactory.CreateMainShiftWithDefaultCategory();
            mainShift.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("hj"), new DateTimePeriod()));
            mainShift.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("er"), new DateTimePeriod()));

            MainShiftActivityLayer layer = new MainShiftActivityLayer(ActivityFactory.CreateActivity("tre"), new DateTimePeriod());

            mainShift.LayerCollection.Add(layer);
            mainShift.LayerCollection.MoveUpLayer(layer);
            Assert.AreEqual(mainShift.LayerCollection[1], layer);
        }

        [Test]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void CannotMoveUpFirstLayer()
        {
            MainShift mainShift = MainShiftFactory.CreateMainShiftWithDefaultCategory();
            MainShiftActivityLayer layer = new MainShiftActivityLayer(ActivityFactory.CreateActivity("tre"), new DateTimePeriod());

            mainShift.LayerCollection.Add(layer);
            mainShift.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("hj"), new DateTimePeriod()));
            mainShift.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("er"), new DateTimePeriod()));

            ((LayerCollection<IActivity>)mainShift.LayerCollection).MoveUpLayer(layer);
            Assert.AreEqual(mainShift.LayerCollection[0], layer);
        }

        [Test]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void CanMoveDownLayer()
        {
            MainShift mainShift = MainShiftFactory.CreateMainShiftWithDefaultCategory();
            MainShiftActivityLayer layer = new MainShiftActivityLayer(ActivityFactory.CreateActivity("index0"), new DateTimePeriod());
            mainShift.LayerCollection.Add(layer);
            mainShift.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("hj"), new DateTimePeriod()));
            mainShift.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("er"), new DateTimePeriod()));

            Assert.IsTrue(mainShift.LayerCollection.CanMoveDownLayer(layer));
            ((LayerCollection<IActivity>)mainShift.LayerCollection).MoveDownLayer(layer);
            Assert.AreEqual(mainShift.LayerCollection[1], layer);
        }

        [Test]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void CannotMoveDownLayerLastLayer()
        {
            MainShift mainShift = MainShiftFactory.CreateMainShiftWithDefaultCategory();
            
            mainShift.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("hj"), new DateTimePeriod()));
            mainShift.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("er"), new DateTimePeriod()));
            MainShiftActivityLayer layer = new MainShiftActivityLayer(ActivityFactory.CreateActivity("index0"), new DateTimePeriod());
            mainShift.LayerCollection.Add(layer);

            ((LayerCollection<IActivity>)mainShift.LayerCollection).MoveDownLayer(layer);
            Assert.AreEqual(mainShift.LayerCollection[2], layer);
        }


        [Test]
        public void VerifyCanMoveUp()
        {
            var mainShift = MainShiftFactory.CreateMainShiftWithDefaultCategory();

            MainShiftActivityLayer layer = new MainShiftActivityLayer(ActivityFactory.CreateActivity("tre"), new DateTimePeriod());
            MainShiftActivityLayer layerNotInCollection = new MainShiftActivityLayer(ActivityFactory.CreateActivity("tre"), new DateTimePeriod());

            mainShift.LayerCollection.Add(layer);
            Assert.IsFalse(mainShift.LayerCollection.CanMoveUpLayer(layer), "Cannot move, only one layer");

            mainShift.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("hj"), new DateTimePeriod()));
            Assert.IsFalse(mainShift.LayerCollection.CanMoveUpLayer(layer), "Cannot move, top layer");

            mainShift.LayerCollection.MoveDownLayer(layer);
            Assert.IsTrue(mainShift.LayerCollection.CanMoveUpLayer(layer));

            Assert.IsFalse(mainShift.LayerCollection.CanMoveUpLayer(layerNotInCollection), "cannot move if not in collection");
            Assert.IsFalse(mainShift.LayerCollection.CanMoveUpLayer(null), "cannot move if null");
        }

        [Test]
        public void VerifyCanMoveDown()
        {
            var mainShift = MainShiftFactory.CreateMainShiftWithDefaultCategory();

            MainShiftActivityLayer layer = new MainShiftActivityLayer(ActivityFactory.CreateActivity("index0"), new DateTimePeriod());
            MainShiftActivityLayer layerNotInCollection = new MainShiftActivityLayer(ActivityFactory.CreateActivity("tre"), new DateTimePeriod());
            
            mainShift.LayerCollection.Add(layer);
            Assert.IsFalse(mainShift.LayerCollection.CanMoveDownLayer(layer), "Cannot move, only one layer");

            mainShift.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("hj"), new DateTimePeriod()));
            Assert.IsTrue(mainShift.LayerCollection.CanMoveDownLayer(layer));

            mainShift.LayerCollection.MoveDownLayer(layer);
            Assert.IsFalse(mainShift.LayerCollection.CanMoveDownLayer(layer), "cannot move down when last");

            Assert.IsFalse(mainShift.LayerCollection.CanMoveDownLayer(layerNotInCollection), "cannot move if not in collection");
            Assert.IsFalse(mainShift.LayerCollection.CanMoveDownLayer(null), "cannot move if null");
        }
      
        [Test]
        public void VerifyAddRange()
        {
            ActivityLayer layer1 = new ActivityLayer(ActivityFactory.CreateActivity("sdf"), new DateTimePeriod(2000, 1, 1, 2002, 1, 1));
            ActivityLayer layer2 = new ActivityLayer(ActivityFactory.CreateActivity("sdf"), new DateTimePeriod(2000, 1, 1, 2002, 1, 1));
            ActivityLayer layer3 = new ActivityLayer(ActivityFactory.CreateActivity("sdf"), new DateTimePeriod(2000, 1, 1, 2002, 1, 1));
            ActivityLayer layer4 = new ActivityLayer(ActivityFactory.CreateActivity("sdf"), new DateTimePeriod(2000, 1, 1, 2002, 1, 1));

            LayerCollection<IActivity> newLayerCollection = new LayerCollection<IActivity>();
            newLayerCollection.Add(layer2);
            newLayerCollection.Add(layer3);
            newLayerCollection.Add(layer4);

            target.Add(layer1);
            target.AddRange(newLayerCollection);

            Assert.AreEqual(4, target.Count);
            Assert.IsTrue(target.Contains(layer1));
            Assert.IsTrue(target.Contains(layer2));
            Assert.IsTrue(target.Contains(layer3));
            Assert.IsTrue(target.Contains(layer4));
        }
    }
}