using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.DomainTest.Common;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class EntityConverterTest
    {

        #region Variables

        // Variable to hold object to be tested for reuse by init functions
        private EntityContainer<EntityConverterTestClass> _converter;
        private EntityConverterTestClass _entity;

        #endregion

        #region SetUp and TearDown

        [SetUp]
        public void TestInit()
        {
            _entity = new EntityConverterTestClass("Name", "Desc");
            _converter = new EntityContainer<EntityConverterTestClass>();
            _converter.ContainedEntity = _entity;
        }

        [TearDown]
        public void TestDispose()
        {
            _entity = null;
            _converter = null;
        }

        #endregion

        #region Static Method Tests

        [Test]
        public void VerifyConvert()
        {
            object res = EntityConverter.ConvertToOther<EntityConverterTestClass, EntityContainer<EntityConverterTestClass>>(_entity);
            Assert.IsInstanceOf<EntityContainer<EntityConverterTestClass>>(res);

            res = EntityConverter.ConvertToEntity<EntityConverterTestClass, EntityContainer<EntityConverterTestClass>>(_converter);
            Assert.IsInstanceOf<EntityConverterTestClass>(res);

        
        }
        #endregion
    }
}
