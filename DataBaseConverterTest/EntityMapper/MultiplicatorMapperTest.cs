using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for MultiplicatorMapper
    /// </summary>
    [TestFixture]
    public class MultiplicatorMapperTest : MapperTest<Multiplicator>
    {
        private MultiplicatorMapper _target;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _target = new MultiplicatorMapper(new MappedObjectPair());
        }

        /// <summary>
        /// Determines whether this type [can validate number of properties].
        /// </summary>
        [Test]
        public void CanValidateNumberOfProperties()
        {
            Assert.AreEqual(13, PropertyCounter.CountProperties(typeof (MultiplicatorDefinitionSet)));
        }

        /// <summary>
        /// Determines whether this instance [can map overtime activity6x].
        /// </summary>
        [Test]
        public void CanMapOvertimeActivity6XAndShrinkName()
        {
            IMultiplicator multiplicator = new Multiplicator(MultiplicatorType.Overtime);
            multiplicator.Description = new Description("My Multiplicator");

            IMultiplicatorDefinitionSet newMultiplicatorDefinitionSet = _target.Map(multiplicator);
            
            Assert.AreEqual(multiplicator.Description.Name, newMultiplicatorDefinitionSet.Name);
            Assert.AreEqual(MultiplicatorType.Overtime, newMultiplicatorDefinitionSet.MultiplicatorType);
            Assert.AreEqual(7,newMultiplicatorDefinitionSet.DefinitionCollection.Count);
        }

        [Test]
        public void CanHandleDeleted()
        {
            IMultiplicator multiplicator = new Multiplicator(MultiplicatorType.Overtime);
            multiplicator.Description = new Description("My Multiplicator");
            ((IDeleteTag) multiplicator).SetDeleted();

            IMultiplicatorDefinitionSet newMultiplicatorDefinitionSet = _target.Map(multiplicator);
            Assert.IsTrue(((IDeleteTag)newMultiplicatorDefinitionSet).IsDeleted);
            Assert.AreEqual(7, newMultiplicatorDefinitionSet.DefinitionCollection.Count);
        }

        protected override int NumberOfPropertiesToConvert
        {
            get { return 15; }
        }
    }
}