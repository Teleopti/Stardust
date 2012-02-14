using System;
using System.Drawing;
using Domain;
using Infrastructure;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;
using EmploymentType=Domain.EmploymentType;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for the WorkShiftMapper
    /// </summary>
    [TestFixture]
    public class WorkShiftMapperTest : MapperTest<global::Domain.WorkShift>
    {
        protected override int NumberOfPropertiesToConvert
        {
            get { return 7; }
        }

        [Test]
        public void CanMapWorkShift6X()
        {
            global::Domain.ShiftCategory oldShiftCat1 = new global::Domain.ShiftCategory(12, "Morning", "MO", Color.AliceBlue, true, true, 0);

            Unit oldUnit = new Unit(-1, "Test", false, false, null, null, false);

            EmploymentType empType =
                new EmploymentType(-1, "Full time", 3, new TimeSpan(11, 0, 0), new TimeSpan(36, 0, 0),
                                   new TimeSpan(2, 0, 0));

            AgentDayFactory agdFactory = new AgentDayFactory();
            global::Domain.WorkShift oldWorkShift2 = agdFactory.WorkShift();

            global::Domain.ShiftClass oldShiftClass1 =
                new global::Domain.ShiftClass(-1, "8-17", ShiftType.Good, oldUnit, oldShiftCat1, empType, null, null,
                                              null, null, null, new TimeSpan(16, 30, 0), new TimeSpan(17, 30, 0),
                                              new TimeSpan(0, 5, 0), oldWorkShift2.LayerCollection[0].LayerActivity);

            ICccListCollection<global::Domain.ActivityLayer> activityLayerCollection =
                new CccListCollection<global::Domain.ActivityLayer>(agdFactory.ActivityLayerList(),CollectionType.Changeable);

            global::Domain.WorkShift oldWorkShift1 =
                new global::Domain.WorkShift(-1, "AA1500", oldShiftClass1,activityLayerCollection, false, oldShiftCat1);

            MappedObjectPair mappedObjectPair = new MappedObjectPair();
            mappedObjectPair.Activity = agdFactory.ActPairList;
            mappedObjectPair.ShiftCategory = new ObjectPairCollection<ShiftCategory, IShiftCategory>();
            mappedObjectPair.ShiftCategory.Add(oldShiftCat1, new Domain.Scheduling.ShiftCategory("fsdfsd"));

            WorkShiftMapper msMap = new WorkShiftMapper(mappedObjectPair, new CccTimeZoneInfo(TimeZoneInfo.Utc));
            IWorkShift newShift1 = msMap.Map(oldWorkShift1);
            IWorkShift newShift2 = msMap.Map(oldWorkShift2);

            Assert.IsNull(newShift2);
            Assert.AreEqual(2, newShift1.LayerCollection.Count);
        }

        /// <summary>
        /// Determines whether this instance [can map work shift with start day before].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-31
        /// </remarks>
        [Test]
        public void CanMapWorkShiftWithStartDayBefore()
        {
            global::Domain.ShiftCategory oldShiftCat1 = new global::Domain.ShiftCategory(12, "Morning", "MO", Color.AliceBlue, true, true, 0);

            Unit oldUnit = new Unit(-1, "Test", false, false, null, null, false);

            EmploymentType empType =
                new EmploymentType(-1, "Full time", 3, new TimeSpan(11, 0, 0), new TimeSpan(36, 0, 0),
                                   new TimeSpan(2, 0, 0));

            AgentDayFactory agdFactory = new AgentDayFactory();

            ICccListCollection<global::Domain.ActivityLayer> activityLayerCollection =
                new CccListCollection<global::Domain.ActivityLayer>(agdFactory.ActivityLayerList(), CollectionType.Changeable);

            
            global::Domain.ActivityLayer actLayer1 = new global::Domain.ActivityLayer( activityLayerCollection[0].Period.Shift(new TimeSpan(-11, 0, 0)), activityLayerCollection[0].LayerActivity);
            activityLayerCollection[0] = actLayer1;
            global::Domain.ActivityLayer actLayer2 = new global::Domain.ActivityLayer(activityLayerCollection[1].Period.Shift(new TimeSpan(-11, 0, 0)), activityLayerCollection[1].LayerActivity);
            activityLayerCollection[1] = actLayer2;
            global::Domain.ActivityLayer actLayer3 = new global::Domain.ActivityLayer(activityLayerCollection[2].Period.Shift(new TimeSpan(-11, 0, 0)), activityLayerCollection[2].LayerActivity);
            activityLayerCollection[2] = actLayer3;

            global::Domain.ShiftClass oldShiftClass1 =
                new global::Domain.ShiftClass(-1, "8-17", ShiftType.Good, oldUnit, oldShiftCat1, empType, null, null,
                                              null, null, null, new TimeSpan(16, 30, 0), new TimeSpan(17, 30, 0),
                                              new TimeSpan(0, 5, 0), activityLayerCollection[0].LayerActivity);

            global::Domain.WorkShift oldWorkShift1 =
                new global::Domain.WorkShift(-1, "AA1500", oldShiftClass1, activityLayerCollection, false, oldShiftCat1);

            MappedObjectPair mappedObjectPair = new MappedObjectPair();
            mappedObjectPair.Activity = agdFactory.ActPairList;
            mappedObjectPair.ShiftCategory = new ObjectPairCollection<ShiftCategory, IShiftCategory>();
            mappedObjectPair.ShiftCategory.Add(oldShiftCat1, new Domain.Scheduling.ShiftCategory("fsdfsd"));
            
            WorkShiftMapper msMap = new WorkShiftMapper(mappedObjectPair, new CccTimeZoneInfo(TimeZoneInfo.Utc));
            IWorkShift newShift1 = msMap.Map(oldWorkShift1);

            Assert.AreEqual(2, newShift1.LayerCollection.Count);
        }
    }
}