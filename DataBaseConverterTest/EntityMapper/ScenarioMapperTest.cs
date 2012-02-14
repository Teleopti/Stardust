using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for Scenario
    /// </summary>
    [TestFixture]
    public class ScenarioMapperTest : MapperTest<global::Domain.Scenario>
    {
        private ScenarioMapper _target;
        private string _oldName1;
        private string _oldName2;
        private MappedObjectPair _mappedObjectPair;

        protected override int NumberOfPropertiesToConvert
        {
            get { return 4; }
        }

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _mappedObjectPair = new MappedObjectPair();
            _target = new ScenarioMapper(_mappedObjectPair);
        }

        /// <summary>
        /// Verifies that creation and properties works
        /// </summary>
        [Test]
        public void CanCreateMapperObject()
        {
            Assert.IsNotNull(_target);
            Assert.AreSame(_mappedObjectPair, _target.MappedObjectPair);
        }

        /// <summary>
        /// Verifies that this type [can validate number of properties].
        /// </summary>
        [Test]
        public void CanMapOldObjToNewObj()
        {
            _oldName1 = "High";
            _oldName2 = "Deafault";

            global::Domain.Scenario oldScenarioDefault = new global::Domain.Scenario(3, _oldName2,
                                                                              true,
                                                                              false);
            global::Domain.Scenario oldScenario = new global::Domain.Scenario(2, _oldName1,
                                                                              false,
                                                                              false);
            
            IScenario newScenarioDefault = _target.Map(oldScenarioDefault);
            IScenario newScenario = _target.Map(oldScenario);
            Assert.AreEqual(oldScenario.Name, newScenario.Description.Name);
            Assert.AreEqual(oldScenario.AuditTrail, newScenario.AuditTrail);
            Assert.AreEqual(oldScenario.DefaultScenario, newScenario.DefaultScenario);
            Assert.AreEqual(oldScenario.DefaultScenario, newScenario.EnableReporting);
            Assert.AreEqual(oldScenarioDefault.DefaultScenario, newScenarioDefault.EnableReporting);
        }
    }
}