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
        /// Verifies that null cannot be added to list.
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void VerifyNullCannotBeAddedToList()
        {
            target.Add(null);
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
            var act = ActivityFactory.CreateActivity("hejhej");
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
            var mainShift = new WorkShift(new ShiftCategory("#"));
            mainShift.LayerCollection.Add(
                new WorkShiftActivityLayer(ActivityFactory.CreateActivity("hj"),
                                                        new DateTimePeriod()));
        }

        /// <summary>
        /// Verifies that valid type could be removed.
        /// </summary>
        [Test]
        public void VerifyValidTypeCouldBeRemoved()
        {
            var newMainShift =
                new WorkShiftActivityLayer(ActivityFactory.CreateActivity("hj"), new DateTimePeriod());

						var mainShift = new WorkShift(new ShiftCategory("#"));
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
					var mainShift = new WorkShift(new ShiftCategory("#"));
            mainShift.LayerCollection.Add(new WorkShiftActivityLayer(ActivityFactory.CreateActivity("hj"), new DateTimePeriod()));
						mainShift.LayerCollection.Add(new WorkShiftActivityLayer(ActivityFactory.CreateActivity("er"), new DateTimePeriod()));

            Assert.AreEqual(2, mainShift.LayerCollection.Count);
            mainShift.LayerCollection.Clear();
            Assert.AreEqual(0,mainShift.LayerCollection.Count);
        }

        [Test]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void CanAddLayerAtIndexedPosition()
        {
					var mainShift = new WorkShift(new ShiftCategory("#"));
					mainShift.LayerCollection.Add(new WorkShiftActivityLayer(ActivityFactory.CreateActivity("hj"), new DateTimePeriod()));
					mainShift.LayerCollection.Add(new WorkShiftActivityLayer(ActivityFactory.CreateActivity("er"), new DateTimePeriod()));

					var layer = new WorkShiftActivityLayer(ActivityFactory.CreateActivity("index0"), new DateTimePeriod());

            mainShift.LayerCollection.Insert(0, layer);
            Assert.AreEqual(mainShift.LayerCollection[0],layer);
        }

        [Test]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void CanMoveUpLayer()
        {
					var mainShift = new WorkShift(new ShiftCategory("#"));
					mainShift.LayerCollection.Add(new WorkShiftActivityLayer(ActivityFactory.CreateActivity("hj"), new DateTimePeriod()));
					mainShift.LayerCollection.Add(new WorkShiftActivityLayer(ActivityFactory.CreateActivity("er"), new DateTimePeriod()));

					var layer = new WorkShiftActivityLayer(ActivityFactory.CreateActivity("tre"), new DateTimePeriod());

            mainShift.LayerCollection.Add(layer);
            mainShift.LayerCollection.MoveUpLayer(layer);
            Assert.AreEqual(mainShift.LayerCollection[1], layer);
        }

        [Test]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void CannotMoveUpFirstLayer()
        {
					var mainShift = new WorkShift(new ShiftCategory("#"));
					var layer = new WorkShiftActivityLayer(ActivityFactory.CreateActivity("tre"), new DateTimePeriod());

            mainShift.LayerCollection.Add(layer);
						mainShift.LayerCollection.Add(new WorkShiftActivityLayer(ActivityFactory.CreateActivity("hj"), new DateTimePeriod()));
						mainShift.LayerCollection.Add(new WorkShiftActivityLayer(ActivityFactory.CreateActivity("er"), new DateTimePeriod()));

            ((LayerCollection<IActivity>)mainShift.LayerCollection).MoveUpLayer(layer);
            Assert.AreEqual(mainShift.LayerCollection[0], layer);
        }

        [Test]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void CanMoveDownLayer()
        {
					var mainShift = new WorkShift(new ShiftCategory("#"));
					var layer = new WorkShiftActivityLayer(ActivityFactory.CreateActivity("index0"), new DateTimePeriod());
            mainShift.LayerCollection.Add(layer);
						mainShift.LayerCollection.Add(new WorkShiftActivityLayer(ActivityFactory.CreateActivity("hj"), new DateTimePeriod()));
						mainShift.LayerCollection.Add(new WorkShiftActivityLayer(ActivityFactory.CreateActivity("er"), new DateTimePeriod()));

            Assert.IsTrue(mainShift.LayerCollection.CanMoveDownLayer(layer));
            ((LayerCollection<IActivity>)mainShift.LayerCollection).MoveDownLayer(layer);
            Assert.AreEqual(mainShift.LayerCollection[1], layer);
        }

        [Test]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void CannotMoveDownLayerLastLayer()
        {
					var mainShift = new WorkShift(new ShiftCategory("#"));

					mainShift.LayerCollection.Add(new WorkShiftActivityLayer(ActivityFactory.CreateActivity("hj"), new DateTimePeriod()));
					mainShift.LayerCollection.Add(new WorkShiftActivityLayer(ActivityFactory.CreateActivity("er"), new DateTimePeriod()));
					var layer = new WorkShiftActivityLayer(ActivityFactory.CreateActivity("index0"), new DateTimePeriod());
            mainShift.LayerCollection.Add(layer);

            ((LayerCollection<IActivity>)mainShift.LayerCollection).MoveDownLayer(layer);
            Assert.AreEqual(mainShift.LayerCollection[2], layer);
        }

        [Test]
        public void VerifyCanMoveUp()
        {
					var mainShift = new WorkShift(new ShiftCategory("#"));

					var layer = new WorkShiftActivityLayer(ActivityFactory.CreateActivity("tre"), new DateTimePeriod());
					var layerNotInCollection = new WorkShiftActivityLayer(ActivityFactory.CreateActivity("tre"), new DateTimePeriod());

            mainShift.LayerCollection.Add(layer);
            Assert.IsFalse(mainShift.LayerCollection.CanMoveUpLayer(layer), "Cannot move, only one layer");

						mainShift.LayerCollection.Add(new WorkShiftActivityLayer(ActivityFactory.CreateActivity("hj"), new DateTimePeriod()));
            Assert.IsFalse(mainShift.LayerCollection.CanMoveUpLayer(layer), "Cannot move, top layer");

            mainShift.LayerCollection.MoveDownLayer(layer);
            Assert.IsTrue(mainShift.LayerCollection.CanMoveUpLayer(layer));

            Assert.IsFalse(mainShift.LayerCollection.CanMoveUpLayer(layerNotInCollection), "cannot move if not in collection");
            Assert.IsFalse(mainShift.LayerCollection.CanMoveUpLayer(null), "cannot move if null");
        }

        [Test]
        public void VerifyCanMoveDown()
        {
					var mainShift = new WorkShift(new ShiftCategory("#"));

					var layer = new WorkShiftActivityLayer(ActivityFactory.CreateActivity("index0"), new DateTimePeriod());
					var layerNotInCollection = new WorkShiftActivityLayer(ActivityFactory.CreateActivity("tre"), new DateTimePeriod());
            
            mainShift.LayerCollection.Add(layer);
            Assert.IsFalse(mainShift.LayerCollection.CanMoveDownLayer(layer), "Cannot move, only one layer");

						mainShift.LayerCollection.Add(new WorkShiftActivityLayer(ActivityFactory.CreateActivity("hj"), new DateTimePeriod()));
            Assert.IsTrue(mainShift.LayerCollection.CanMoveDownLayer(layer));

            mainShift.LayerCollection.MoveDownLayer(layer);
            Assert.IsFalse(mainShift.LayerCollection.CanMoveDownLayer(layer), "cannot move down when last");

            Assert.IsFalse(mainShift.LayerCollection.CanMoveDownLayer(layerNotInCollection), "cannot move if not in collection");
            Assert.IsFalse(mainShift.LayerCollection.CanMoveDownLayer(null), "cannot move if null");
        }
    }
}