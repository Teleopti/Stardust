using System;
using System.Collections.ObjectModel;
using System.Drawing;
using Domain;
using Infrastructure;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for ShiftClassificationMapper
    /// </summary>
    [TestFixture]
    public class ShiftClassificationMapperTest : MapperTest<global::Domain.ShiftClass>
    {
        private ShiftClassificationMapper target;

        protected override int NumberOfPropertiesToConvert
        {
            get { return 15; }
        }

        /// <summary>
        /// Verifies that creation and properties works
        /// </summary>
        [Test]
        public void CanCreateMapperObject()
        {
            MappedObjectPair mappedObjectPair = new MappedObjectPair();
            target = new ShiftClassificationMapper(mappedObjectPair,TimeZoneInfo.Utc,new Collection<global::Domain.WorkShift>());
            Assert.IsNotNull(target);
        }

        /// <summary>
        /// Verifies that this type [can map old empty shift class to new shift classification].
        /// </summary>
        [Test]
        public void CanMapOldObjToNewObj()
        {
            string oldName = "8-9";

            global::Domain.Unit oldUnit = new global::Domain.Unit(1, "TestSite", false, false, null, null, false);
            Site newSite = new Site("TestSite");

            ObjectPairCollection<Unit, Site> sitePairList = new ObjectPairCollection<Unit, Site>();
            sitePairList.Add(oldUnit, newSite);

            global::Domain.ShiftCategory oldShiftCategory =
                new global::Domain.ShiftCategory(-1, "TestCategory", "Test", Color.White, true, true);
            Domain.Scheduling.ShiftCategory newShiftCat1 = new Domain.Scheduling.ShiftCategory("TestCategory");

            ObjectPairCollection<global::Domain.ShiftCategory, Domain.Scheduling.ShiftCategory> shiftCatPairList =
                new ObjectPairCollection<global::Domain.ShiftCategory, Domain.Scheduling.ShiftCategory>();
            shiftCatPairList.Add(oldShiftCategory, newShiftCat1);
            
            global::Domain.EmploymentType empType =
                new global::Domain.EmploymentType(-1, "Test employment type", 1, new TimeSpan(11, 0, 0), new TimeSpan(24, 0, 0),
                                   new TimeSpan(2, 0, 0));

            Contract newContract = new Contract("NewContract");
            ObjectPairCollection<WorktimeType, Contract> contractPairList =
                new ObjectPairCollection<WorktimeType, Contract>();
            contractPairList.Add((WorktimeType)1, newContract);

            MappedObjectPair mappedObjectPair = new MappedObjectPair();
            mappedObjectPair.ShiftCategory = shiftCatPairList;
            mappedObjectPair.Site = sitePairList;
            mappedObjectPair.Contract = contractPairList;

            ShiftClassificationMapper classificationMapper = new ShiftClassificationMapper(mappedObjectPair, TimeZoneInfo.Utc, new Collection<global::Domain.WorkShift>());
            global::Domain.ShiftClass oldShiftClass =
                new global::Domain.ShiftClass(12, oldName, global::Domain.ShiftType.Preferred, oldUnit, oldShiftCategory, empType, null,
                               null,null,null,null,new TimeSpan(16,30,0), new TimeSpan(17,30,0),new TimeSpan(0,5,0), null);

            ShiftClassification newClassification = classificationMapper.Map(oldShiftClass);
            Assert.AreEqual(oldShiftClass.Name, newClassification.Description.Name);
        }

        /// <summary>
        /// Determines whether this instance [can map old obj to new obj with all properties].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-31
        /// </remarks>
        [Test]
        public void CanMapOldObjToNewObjWithAllProperties()
        {
            string oldName = "8-9";

            global::Domain.ShiftCategory oldShiftCat1 = new global::Domain.ShiftCategory(12, "Morning", "MO", Color.AliceBlue, true, true);

            Domain.Scheduling.ShiftCategory newShiftCat1 = new Domain.Scheduling.ShiftCategory("Morning");

            ObjectPairCollection<global::Domain.ShiftCategory, Domain.Scheduling.ShiftCategory> shiftCatPairList =
                new ObjectPairCollection<global::Domain.ShiftCategory, Domain.Scheduling.ShiftCategory>();
            shiftCatPairList.Add(oldShiftCat1, newShiftCat1);

            Unit oldUnit = new Unit(-1, "Test", false, false, null, null, false);
            Site newSite = new Site("TestSite");

            ObjectPairCollection<Unit, Site> sitePairList = new ObjectPairCollection<Unit, Site>();
            sitePairList.Add(oldUnit, newSite);

            global::Domain.EmploymentType empType =
                new global::Domain.EmploymentType(-1, "Test employment type", 1, new TimeSpan(11, 0, 0), new TimeSpan(24, 0, 0),
                                   new TimeSpan(2, 0, 0));

            global::Domain.StartLengthDefinition startLengthDefinition =
                new global::Domain.StartLengthDefinition(new TimeSpan(7, 30, 0), new TimeSpan(8, 30, 0),
                                                         new TimeSpan(0, 5, 0), new TimeSpan(7, 30, 0),
                                                         new TimeSpan(8, 30, 0), new TimeSpan(0, 5, 0));

            global::Domain.ShiftClass oldShiftClass1 =
                new global::Domain.ShiftClass(12, oldName, global::Domain.ShiftType.Preferred, oldUnit, oldShiftCat1, empType, null,
                               null, null, null, startLengthDefinition, new TimeSpan(16, 30, 0), new TimeSpan(17, 30, 0), new TimeSpan(0, 5, 0), null);

            AgentDayFactory agdFactory = new AgentDayFactory();
            ICccListCollection<global::Domain.ActivityLayer> activityLayerCollection =
                new CccListCollection<global::Domain.ActivityLayer>(agdFactory.ActivityLayerList(), CollectionType.Changeable);

            global::Domain.WorkShift oldWorkShift1 =
                new global::Domain.WorkShift(-1, "AA1500", oldShiftClass1, activityLayerCollection, false, oldShiftCat1);

            Collection<global::Domain.WorkShift> oldWorkShiftCollection = new Collection<global::Domain.WorkShift>();
            oldWorkShiftCollection.Add(oldWorkShift1);

            Contract newContract = new Contract("NewContract");
            ObjectPairCollection<WorktimeType, Contract> contractPairList =
                new ObjectPairCollection<WorktimeType, Contract>();
            contractPairList.Add((WorktimeType) 1, newContract);

            MappedObjectPair mappedObjectPair = new MappedObjectPair();
            mappedObjectPair.Site = sitePairList;
            mappedObjectPair.Activity = agdFactory.ActPairList;
            mappedObjectPair.ShiftCategory = shiftCatPairList;
            mappedObjectPair.Contract = contractPairList;
            ShiftClassificationMapper classificationMapper = new ShiftClassificationMapper(mappedObjectPair, TimeZoneInfo.Utc, oldWorkShiftCollection);

            ShiftClassification newClassification = classificationMapper.Map(oldShiftClass1);
            Assert.AreEqual(oldShiftClass1.Name, newClassification.Description.Name);
            Assert.AreEqual(1, newClassification.WorkShiftCollection.Count);
        }
    }
}