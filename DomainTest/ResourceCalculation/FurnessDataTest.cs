using NUnit.Framework;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Ccc.Secrets.Furness;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class FurnessDataTest
    {
        private FurnessData _target;

        [SetUp]
        public void Setup()
        {
            _target = FurnessDataFactory.CreateFurnessDataForTestSet1(); 
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyProductionDemandProperty()
        {
            Assert.AreEqual(3, _target.ProductionDemand().GetLongLength(0));
            Assert.AreEqual(8, _target.ProductionDemand()[2]);
        }

        [Test]
        public void VerifyProducerResourceProperty()
        {
            Assert.AreEqual(3, _target.ProducerResources().GetLongLength(0));
            Assert.AreEqual(8, _target.ProducerResources()[2]);
        }

        [Test]
        public void VerifyProductivityMatrixProperty()
        {
            Assert.AreEqual(3, _target.ProductivityMatrix().GetLongLength(0));
            Assert.AreEqual(3, _target.ProductivityMatrix().GetLongLength(1));
            Assert.AreEqual(1.5, _target.ProductivityMatrix()[0, 0]);
            Assert.AreEqual(1.5, _target.ProductivityMatrix()[2, 2]);
        }

        [Test]
        public void VerifyResourceMatrixProperty()
        {
            Assert.AreEqual(3, _target.ResourceMatrix().GetLongLength(0));
            Assert.AreEqual(3, _target.ResourceMatrix().GetLongLength(1));
            Assert.AreEqual(3, _target.ProductTypes);
            Assert.AreEqual(3, _target.Producers);
            Assert.AreEqual(1, _target.ResourceMatrix()[0, 0]);
            Assert.AreEqual(1, _target.ResourceMatrix()[2, 2]);
        }

        [Test]
        public void VerifyTotalProperties()
        {
            Assert.AreEqual(9, _target.TotalProduction());
            Assert.AreEqual(25, _target.TotalProductionDemand());
            Assert.AreEqual(21, _target.TotalProducerResources());
        }
    }
}
