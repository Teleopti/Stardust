using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for LayerStartDateTimeSorter
    /// </summary>
    [TestFixture]
    public class LayerSorterTest
    {
        /// <summary>
        /// Verifies that the returned list is sorted in ascending order.
        /// </summary>
        /// 
        [Test]
        public void CanSortListAccordingToStartDateTimeAscending()
        {
            LayerCollection<IActivity> sortedList = new LayerCollection<IActivity>();
            LayerCollection<IActivity> nonSortedList = new LayerCollection<IActivity>();
            IActivity act1 = ActivityFactory.CreateActivity("Telefon");
            IActivity act2 = ActivityFactory.CreateActivity("Rast");
            IActivity act3 = ActivityFactory.CreateActivity("Möte");
            IActivity act4 = ActivityFactory.CreateActivity("Bön");

            DateTimePeriod period1 =
                new DateTimePeriod(new DateTime(2001, 1, 1, 10, 00, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 11, 00, 0, DateTimeKind.Utc));
            DateTimePeriod period2 =
                new DateTimePeriod(new DateTime(2001, 1, 1, 11, 00, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 12, 00, 0, DateTimeKind.Utc));
            DateTimePeriod period3 =
                new DateTimePeriod(new DateTime(2001, 1, 1, 12, 00, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 16, 00, 0, DateTimeKind.Utc));
            DateTimePeriod period4 =
                new DateTimePeriod(new DateTime(2001, 1, 1, 16, 00, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 17, 00, 0, DateTimeKind.Utc));

            ActivityLayer layer1 = new ActivityLayer(act1, period1);
            sortedList.Add(layer1);
            ActivityLayer layer2 = new ActivityLayer(act2, period2);
            sortedList.Add(layer2);
            ActivityLayer layer3 = new ActivityLayer(act3, period3);
            sortedList.Add(layer3);
            ActivityLayer layer4 = new ActivityLayer(act4, period4);
            sortedList.Add(layer4);

            nonSortedList.Add(layer3);
            nonSortedList.Add(layer1);
            nonSortedList.Add(layer4);
            nonSortedList.Add(layer2);

            LayerStartDateTimeSorter<IActivity> s = new LayerStartDateTimeSorter<IActivity>(true);
            nonSortedList.Sort(s);

            Assert.AreEqual(sortedList[0].Period.StartDateTime, nonSortedList[0].Period.StartDateTime);
            Assert.AreEqual(sortedList[1].Period.StartDateTime, nonSortedList[1].Period.StartDateTime);
            Assert.AreEqual(sortedList[2].Period.StartDateTime, nonSortedList[2].Period.StartDateTime);
            Assert.AreEqual(sortedList[3].Period.StartDateTime, nonSortedList[3].Period.StartDateTime);
        }

        /// <summary>
        /// Verifies that the returned list is sorted in descending order.
        /// </summary>
        /// 
        [Test]
        public void CanSortListAccordingToStartDateTimeDescending()
        {
            LayerCollection<IActivity> sortedList = new LayerCollection<IActivity>();
            LayerCollection<IActivity> nonSortedList = new LayerCollection<IActivity>();
            IActivity act1 = ActivityFactory.CreateActivity("Telefon");
            IActivity act2 = ActivityFactory.CreateActivity("Rast");
            IActivity act3 = ActivityFactory.CreateActivity("Möte");
            IActivity act4 = ActivityFactory.CreateActivity("Bön");

            DateTimePeriod period1 =
                new DateTimePeriod(new DateTime(2001, 1, 1, 10, 00, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 11, 00, 0, DateTimeKind.Utc));
            DateTimePeriod period2 =
                new DateTimePeriod(new DateTime(2001, 1, 1, 11, 00, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 12, 00, 0, DateTimeKind.Utc));
            DateTimePeriod period3 =
                new DateTimePeriod(new DateTime(2001, 1, 1, 12, 00, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 16, 00, 0, DateTimeKind.Utc));
            DateTimePeriod period4 =
                new DateTimePeriod(new DateTime(2001, 1, 1, 16, 00, 0, DateTimeKind.Utc),
                                   new DateTime(2001, 1, 1, 17, 00, 0, DateTimeKind.Utc));

            ActivityLayer layer4 = new ActivityLayer(act1, period4);
            sortedList.Add(layer4);
            ActivityLayer layer3 = new ActivityLayer(act2, period3);
            sortedList.Add(layer3);
            ActivityLayer layer2 = new ActivityLayer(act3, period2);
            sortedList.Add(layer2);
            ActivityLayer layer1 = new ActivityLayer(act4, period1);
            sortedList.Add(layer1);

            nonSortedList.Add(layer3);
            nonSortedList.Add(layer1);
            nonSortedList.Add(layer4);
            nonSortedList.Add(layer2);

            LayerStartDateTimeSorter<IActivity> s = new LayerStartDateTimeSorter<IActivity>(false);
            nonSortedList.Sort(s);

            Assert.AreEqual(sortedList[0].Period.StartDateTime, nonSortedList[0].Period.StartDateTime);
            Assert.AreEqual(sortedList[1].Period.StartDateTime, nonSortedList[1].Period.StartDateTime);
            Assert.AreEqual(sortedList[2].Period.StartDateTime, nonSortedList[2].Period.StartDateTime);
            Assert.AreEqual(sortedList[3].Period.StartDateTime, nonSortedList[3].Period.StartDateTime);
        }
    }
}