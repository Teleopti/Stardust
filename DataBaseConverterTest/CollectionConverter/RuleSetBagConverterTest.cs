using System;
using Domain;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using System.Drawing;
using Infrastructure;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverterTest.CollectionConverter
{
    [TestFixture]
    public class RuleSetBagConverterTest
    {
        private RuleSetBagConverter target;
        private WorkShiftRuleSetConverter target2;
        private IUnitOfWork uow;
        private MappedObjectPair mappedObjectPair;
        private Mapper<IRuleSetBag, FakeOldEntityRuleSetBag> mapper;
        private Mapper<IWorkShiftRuleSet, global::Domain.ShiftClass> mapper2;
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            uow = mocks.StrictMock<IUnitOfWork>();
            mappedObjectPair = new MappedObjectPair();
            mapper = mocks.StrictMock<Mapper<IRuleSetBag, FakeOldEntityRuleSetBag>>(mappedObjectPair, new CccTimeZoneInfo(TimeZoneInfo.Local));
            mapper2 = mocks.StrictMock<Mapper<IWorkShiftRuleSet, global::Domain.ShiftClass>>(mappedObjectPair, new CccTimeZoneInfo(TimeZoneInfo.Local));
            target = new RuleSetBagConverter(uow, mapper, mappedObjectPair);
            target2 = new WorkShiftRuleSetConverter(uow, mapper2);
        }

        [Test]
        public void VerifyGetRepository()
        {
            Assert.IsNotNull(target.Repository);
        }

        [Test]
        public void VerifyCreateFakeOldEntities()
        {
            mappedObjectPair.Site = new ObjectPairCollection<Unit, ISite>();
            global::Domain.Unit oldUnit = new global::Domain.Unit(7, "oldUnit", false, false, null, null, false);
            global::Domain.Unit allUnit = new global::Domain.Unit(4, "allUnit", true, false, null, null, false);
            global::Domain.Unit oldUnit2 = new global::Domain.Unit(9, "oldUnit2", false, false, null, null, false);

            mappedObjectPair.Site.Add(oldUnit,new Site("ww"));
            mappedObjectPair.Site.Add(oldUnit2, new Site("ww"));
            global::Domain.ShiftCategory oldShiftCategory = new global::Domain.ShiftCategory(1, "oldShiftCategory", "oldSC", Color.Blue, true, true, 0);

            global::Domain.EmploymentType empType =
                new global::Domain.EmploymentType(-1, "Test employment type", 1, new TimeSpan(11, 0, 0), new TimeSpan(24, 0, 0),
                                   new TimeSpan(2, 0, 0));

            global::Domain.ShiftClass oldShiftClass = new global::Domain.ShiftClass(12, "name", global::Domain.ShiftType.Good, oldUnit, oldShiftCategory, empType, null,
                             null, null, null, null, new TimeSpan(16, 30, 0), new TimeSpan(17, 30, 0), new TimeSpan(0, 5, 0), null);

            global::Domain.ShiftClass oldShiftClass2 = new global::Domain.ShiftClass(12, "name2", global::Domain.ShiftType.Good, oldUnit2, oldShiftCategory, empType, null,
                             null, null, null, null, new TimeSpan(16, 30, 0), new TimeSpan(17, 30, 0), new TimeSpan(0, 5, 0), null);

            global::Domain.ShiftClass oldShiftClass3 = new global::Domain.ShiftClass(12, "name3", global::Domain.ShiftType.Good, oldUnit2, oldShiftCategory, empType, null,
                             null, null, null, null, new TimeSpan(16, 30, 0), new TimeSpan(17, 30, 0), new TimeSpan(0, 5, 0), null);

            global::Domain.ShiftClass oldShiftClass4 = new global::Domain.ShiftClass(12, "name3", global::Domain.ShiftType.Good, allUnit, oldShiftCategory, empType, null,
                             null, null, null, null, new TimeSpan(16, 30, 0), new TimeSpan(17, 30, 0), new TimeSpan(0, 5, 0), null);

            Collection<global::Domain.ShiftClass> shiftClasses = new Collection<global::Domain.ShiftClass>();

            shiftClasses.Add(oldShiftClass);
            shiftClasses.Add(oldShiftClass2);
            shiftClasses.Add(oldShiftClass3);
            shiftClasses.Add(oldShiftClass4);

            Collection<FakeOldEntityRuleSetBag> fakeRuleSets = target.CreateFakeOldEntities(shiftClasses, target2);

            Assert.AreEqual(2, fakeRuleSets.Count);
            Assert.AreEqual(2,fakeRuleSets[0].WorkShiftRuleSets.Count);
        }


    }
}
