using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.DatabaseConverterTest.Properties;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for the MainShiftMapper
    /// </summary>
    [TestFixture]
    public class MainShiftMapperTest : MapperTest<global::Domain.WorkShift>
    {
        protected override int NumberOfPropertiesToConvert
        {
            get { return 7; }
        }


        [Test]
        public void CanMapWorkShift6X()
        {
            string longString60 = Resources.LongString60chars;

            global::Domain.ShiftCategory oldShiftCat1 = new global::Domain.ShiftCategory(12, "Morning", "MO", Color.AliceBlue, true, true, 0);
            global::Domain.ShiftCategory oldShiftCat2 = new global::Domain.ShiftCategory(144, "Evening", "EV", Color.BlueViolet, true, true, 0);

            ShiftCategory newShiftCat1 = new ShiftCategory("Morgon");
            ShiftCategory newShiftCat2 = new ShiftCategory("Kväll");
            ShiftCategory newShiftCat3 = new ShiftCategory(("Test" + longString60).Substring(0,50));

            ObjectPairCollection<global::Domain.ShiftCategory, IShiftCategory> shiftCatPairList =
                new ObjectPairCollection<global::Domain.ShiftCategory, IShiftCategory>();

            AgentDayFactory agdFactory = new AgentDayFactory();

            global::Domain.WorkShift oldShift = agdFactory.WorkShift();
            shiftCatPairList.Add(oldShiftCat1, newShiftCat1);
            shiftCatPairList.Add(oldShiftCat2, newShiftCat2);
            shiftCatPairList.Add(oldShift.Category, newShiftCat3);
            MappedObjectPair mappedObjectPair = new MappedObjectPair();
            mappedObjectPair.ShiftCategory = shiftCatPairList;
            mappedObjectPair.Activity = agdFactory.ActPairList;

            MainShiftMapper msMap = new MainShiftMapper(mappedObjectPair, (TimeZoneInfo.Utc), new DateTime(2007, 1, 1));
            var newShift = msMap.Map(oldShift);

            Assert.AreEqual(oldShift.Category.Name.Substring(0,50), newShift.ShiftCategory.Description.Name);
            Assert.AreEqual(2, newShift.LayerCollection.Count);
        }

        /// <summary>
        /// Determines whether this instance [can map work shift with start day before].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-31
        /// </remarks>
        [Test]
        public void CanMapWorkShiftWithStartDayBefore6X()
        {
            string longString60 = Resources.LongString60chars;

            global::Domain.ShiftCategory oldShiftCat1 = new global::Domain.ShiftCategory(12, "Morning", "MO", Color.AliceBlue, true, true, 0);
            global::Domain.ShiftCategory oldShiftCat2 = new global::Domain.ShiftCategory(144, "Evening", "EV", Color.BlueViolet, true, true, 0);

            ShiftCategory newShiftCat1 = new ShiftCategory("Morgon");
            ShiftCategory newShiftCat2 = new ShiftCategory("Kväll");
            ShiftCategory newShiftCat3 = new ShiftCategory(("Test" + longString60).Substring(0, 50));

            ObjectPairCollection<global::Domain.ShiftCategory, IShiftCategory> shiftCatPairList = new ObjectPairCollection<global::Domain.ShiftCategory, IShiftCategory>();

            AgentDayFactory agdFactory = new AgentDayFactory();

            global::Domain.WorkShift oldShift = agdFactory.WorkShift();

            IList<global::Domain.ActivityLayer> activityLayerCollection = oldShift.LayerCollection;
            global::Domain.ActivityLayer actLayer1 = new global::Domain.ActivityLayer(activityLayerCollection[0].Period.Shift(new TimeSpan(-11, 0, 0)), activityLayerCollection[0].LayerActivity);
            activityLayerCollection[0] = actLayer1;
            global::Domain.ActivityLayer actLayer2 = new global::Domain.ActivityLayer(activityLayerCollection[1].Period.Shift(new TimeSpan(-11, 0, 0)), activityLayerCollection[1].LayerActivity);
            activityLayerCollection[1] = actLayer2;
            global::Domain.ActivityLayer actLayer3 = new global::Domain.ActivityLayer(activityLayerCollection[2].Period.Shift(new TimeSpan(-11, 0, 0)), activityLayerCollection[2].LayerActivity);
            activityLayerCollection[2] = actLayer3;

            shiftCatPairList.Add(oldShiftCat1, newShiftCat1);
            shiftCatPairList.Add(oldShiftCat2, newShiftCat2);
            shiftCatPairList.Add(oldShift.Category, newShiftCat3);
            MappedObjectPair mappedObjectPair = new MappedObjectPair();
            mappedObjectPair.ShiftCategory = shiftCatPairList;
            mappedObjectPair.Activity = agdFactory.ActPairList;

            MainShiftMapper msMap = new MainShiftMapper(mappedObjectPair, (TimeZoneInfo.Utc), new DateTime(2007, 1, 1));
            var newShift = msMap.Map(oldShift);

            Assert.AreEqual(oldShift.Category.Name.Substring(0, 50), newShift.ShiftCategory.Description.Name);
            Assert.AreEqual(2, newShift.LayerCollection.Count);
        }
    }
}