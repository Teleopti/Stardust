using NUnit.Framework;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class FurnessDataTest
    {
        [Test]
        public void VerifyConstructor()
		{
			var target = FurnessDataFactory.CreateFurnessDataForTestSet1(); 
            Assert.IsNotNull(target);
        }

        [Test]
        public void VerifyProductionDemandProperty()
		{
			var target = FurnessDataFactory.CreateFurnessDataForTestSet1(); 
            Assert.AreEqual(3, target.ProductionDemand().GetLongLength(0));
            Assert.AreEqual(8, target.ProductionDemand()[2]);
        }

        [Test]
        public void VerifyProducerResourceProperty()
        {
			var target = FurnessDataFactory.CreateFurnessDataForTestSet1(); 
            Assert.AreEqual(3, target.ProducerResources().GetLongLength(0));
            Assert.AreEqual(8, target.ProducerResources()[2]);
        }

        [Test]
        public void VerifyProductivityMatrixProperty()
		{
			var target = FurnessDataFactory.CreateFurnessDataForTestSet1(); 
            Assert.AreEqual(3, target.ProductivityMatrix().GetLongLength(0));
            Assert.AreEqual(3, target.ProductivityMatrix().GetLongLength(1));
            Assert.AreEqual(1.5, target.ProductivityMatrix()[0, 0]);
            Assert.AreEqual(1.5, target.ProductivityMatrix()[2, 2]);
        }

        [Test]
        public void VerifyResourceMatrixProperty()
		{
			var target = FurnessDataFactory.CreateFurnessDataForTestSet1(); 
            Assert.AreEqual(3, target.ResourceMatrix().GetLongLength(0));
            Assert.AreEqual(3, target.ResourceMatrix().GetLongLength(1));
            Assert.AreEqual(3, target.ProductTypes);
            Assert.AreEqual(3, target.Producers);
            Assert.AreEqual(1, target.ResourceMatrix()[0, 0]);
            Assert.AreEqual(1, target.ResourceMatrix()[2, 2]);
        }

        [Test]
        public void VerifyTotalProperties()
		{
			var target = FurnessDataFactory.CreateFurnessDataForTestSet1(); 
            Assert.AreEqual(9, target.TotalProduction());
            Assert.AreEqual(25, target.TotalProductionDemand());
            Assert.AreEqual(21, target.TotalProducerResources());
        }
    }
}
