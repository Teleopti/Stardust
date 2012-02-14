using System;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverterTest.CollectionConverter
{
    [TestFixture]
    public class WorkShiftRuleSetConverterTest
    {
        private WorkShiftRuleSetConverter target;
        private IUnitOfWork uow;
        private MappedObjectPair mappedObjectPair;
        private Mapper<IWorkShiftRuleSet, global::Domain.ShiftClass> mapper;
        private MockRepository mocks;
        private MockRepository mocks2;
        private IWorkShiftTemplateGenerator generator;
        private WorkShiftRuleSet ruleSet;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            mocks2 = new MockRepository();
            uow = mocks.StrictMock<IUnitOfWork>();
            mappedObjectPair = new MappedObjectPair();
            mapper = mocks.StrictMock<Mapper<IWorkShiftRuleSet, global::Domain.ShiftClass>>(mappedObjectPair, new CccTimeZoneInfo(TimeZoneInfo.Local));
            target = new WorkShiftRuleSetConverter(uow, mapper);

            generator = mocks2.StrictMock<IWorkShiftTemplateGenerator>();
            ruleSet = new WorkShiftRuleSet(generator); //, shiftCategory);
        }

        [Test]
        public void VerifyGetRepository()
        {
            Assert.IsNotNull(target.Repository);
        }

        [Test]
        public void VerifyGetNewValue()
        {
            ObjectPairCollection<global::Domain.ShiftClass, IWorkShiftRuleSet> pairCollection = new ObjectPairCollection<global::Domain.ShiftClass, IWorkShiftRuleSet>();
            
            global::Domain.Unit oldUnit = new global::Domain.Unit(1, "oldUnit", false, false, null, null, false);
            global::Domain.ShiftCategory oldShiftCategory = new global::Domain.ShiftCategory(1, "oldShiftCategory", "oldSC", Color.Blue, true, true, 0);
            global::Domain.EmploymentType empType =
                new global::Domain.EmploymentType(-1, "Test employment type", 1, new TimeSpan(11, 0, 0), new TimeSpan(24, 0, 0),
                                   new TimeSpan(2, 0, 0));

            global::Domain.ShiftClass oldShiftClass = new global::Domain.ShiftClass(12, "name", global::Domain.ShiftType.Good, oldUnit, oldShiftCategory, empType, null,
                              null, null, null, null, new TimeSpan(16, 30, 0), new TimeSpan(17, 30, 0), new TimeSpan(0, 5, 0), null);

            pairCollection.Add(oldShiftClass, ruleSet);
     
            Assert.IsNull(target.GetNewValue(oldShiftClass));

            TestingConverter testConverter = new TestingConverter(uow, mapper);
            testConverter.AddPairCollection(pairCollection);

            Assert.AreEqual(ruleSet, testConverter.GetValue(oldShiftClass));
        }

        internal class TestingConverter : WorkShiftRuleSetConverter
        {

            public TestingConverter(IUnitOfWork unitOfWork, Mapper<IWorkShiftRuleSet, global::Domain.ShiftClass> mapper)
                : base(unitOfWork, mapper)
            { }

            public void AddPairCollection(ObjectPairCollection<global::Domain.ShiftClass, IWorkShiftRuleSet> pairCollection)
            {
                base.OnPersisted(pairCollection);
            }

            public IWorkShiftRuleSet GetValue(global::Domain.ShiftClass shiftClass)
            {
                return GetNewValue(shiftClass);
            }
        }
    }
}
