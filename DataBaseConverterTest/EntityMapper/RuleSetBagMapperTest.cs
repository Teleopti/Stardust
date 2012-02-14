using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    [TestFixture]
    public class RuleSetBagMapperTest
    {
        private RuleSetBagMapper _ruleSetBagMapper;
        private FakeOldEntityRuleSetBag _oldRuleSetBag;
        private IRuleSetBag _ruleSetBag;
        private WorkShiftRuleSet _workShiftRuleSet;
        private MockRepository mocks;
        private IWorkShiftTemplateGenerator generator;
        global::Domain.Unit oldUnit;
        global::Domain.EmploymentType empType;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            generator = mocks.StrictMock<IWorkShiftTemplateGenerator>();
            _workShiftRuleSet = new WorkShiftRuleSet(generator);
            oldUnit = new global::Domain.Unit(1, "oldUnit", false, false, null, null, false);

            empType = new global::Domain.EmploymentType(-1, "Test employment type", 1, new TimeSpan(11, 0, 0), 
                                                            new TimeSpan(24, 0, 0),
                                                            new TimeSpan(2, 0, 0));

            _oldRuleSetBag = new FakeOldEntityRuleSetBag(oldUnit, empType);
            _oldRuleSetBag.AddWorkShiftRuleSet(_workShiftRuleSet);
            MappedObjectPair mappedObjectPair = new MappedObjectPair();
            mappedObjectPair.RuleSetBag = new ObjectPairCollection<UnitEmploymentType, IRuleSetBag>();
            _ruleSetBagMapper = new RuleSetBagMapper(mappedObjectPair, new CccTimeZoneInfo(TimeZoneInfo.Utc));
        }

        [Test]
        public void CanCreateMapper()
        {
            _ruleSetBagMapper = new RuleSetBagMapper(new MappedObjectPair(), new CccTimeZoneInfo(TimeZoneInfo.Utc));
            Assert.IsNotNull(_ruleSetBagMapper);
        }

        [Test]
        public void CanMap()
        {
            _ruleSetBag = _ruleSetBagMapper.Map(_oldRuleSetBag);
            Assert.AreEqual(_ruleSetBag.Description.Name, _oldRuleSetBag.Description);
            Assert.AreEqual(_ruleSetBag.RuleSetCollection.Count, _oldRuleSetBag.WorkShiftRuleSets.Count);
        }
    }    
}
