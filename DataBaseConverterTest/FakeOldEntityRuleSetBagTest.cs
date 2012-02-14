using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DatabaseConverter;
using System;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest
{
    /// <summary>
    /// Test for FakeOldEntityRuleSetBagTest
    /// </summary>
    [TestFixture]
    public class FakeOldEntityRuleSetBagTest
    {
        private FakeOldEntityRuleSetBag _target;
        

        [SetUp]
        public void Setup()
        {
            
            global::Domain.Unit unit = new global::Domain.Unit(1, "unit", false, false, null, null, false);
            global::Domain.EmploymentType employmentType = new global::Domain.EmploymentType(1, "employmentType",
                                                            4, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10),
                                                            TimeSpan.FromMinutes(10));

            _target = new FakeOldEntityRuleSetBag(unit, employmentType);
        }

        [Test]
        public void TestCanSetGetProperties()
        {
            MockRepository mocks;
            mocks = new MockRepository();
            IWorkShiftTemplateGenerator generator = mocks.StrictMock<IWorkShiftTemplateGenerator>();
            WorkShiftRuleSet workShiftRuleSet = new WorkShiftRuleSet(generator);
            _target.AddWorkShiftRuleSet(workShiftRuleSet);

            Assert.AreEqual("unit", _target.Unit.Name);
            Assert.AreEqual("employmentType", _target.EmploymentType.Description);
            Assert.AreEqual("unit employmentType", _target.Description);
            Assert.AreEqual(1, _target.WorkShiftRuleSets.Count);
        }
    }
}
