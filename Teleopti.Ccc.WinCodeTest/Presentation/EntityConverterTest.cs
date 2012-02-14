using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.DomainTest.Common;
using Teleopti.Ccc.WinCode.Common.Presentation;

namespace Teleopti.Ccc.WinCodeTest.Common.Presentation
{
    [TestFixture]
    public class EntityConverterTest
    {

        #region Variables

        // Variable to hold object to be tested for reuse by init functions
        private EntityContainer<EntityConverterTestClass> _converter;
        private EntityConverterTestClass _domain;

        #endregion

        #region SetUp and TearDown

        [SetUp]
        public void TestInit()
        {
            _domain = new EntityConverterTestClass("Name", "Desc");
            _converter = new EntityContainer<EntityConverterTestClass>();
            _converter.ContainedEntity = _domain;
        }

        [TearDown]
        public void TestDispose()
        {
            _domain = null;
            _converter = null;
        }

        #endregion

        #region Static Method Tests

        [Test]
        public void VerifyConvert()
        {
            object res = EntityConverter.ConvertToOther<EntityConverterTestClass, EntityContainer<EntityConverterTestClass>>(_domain);
            Assert.IsInstanceOfType(typeof(EntityContainer<EntityConverterTestClass>), res);

            res = EntityConverter.ConvertToEntity<EntityConverterTestClass, EntityContainer<EntityConverterTestClass>>(_converter);
            Assert.IsInstanceOfType(typeof(EntityConverterTestClass), res);

        
        }
        #endregion
    }
}
