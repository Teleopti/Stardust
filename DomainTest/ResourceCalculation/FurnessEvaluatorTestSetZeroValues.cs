using NUnit.Framework;
using Teleopti.Ccc.Domain.Calculation;
using Teleopti.Ccc.Secrets.Furness;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class FurnessEvaluatorTestSetZeroValues
    {
        [Test]
        public void VerifyConstructor()
        {
			FurnessData data = FurnessDataFactory.CreateFurnessDataForTestSetZeroDemand();
            var target = new FurnessEvaluator(data);

            Assert.IsNotNull(target);
            Assert.AreEqual(3, target.Data.ProductTypes);
            Assert.AreEqual(3, target.Data.Producers);
        }

        [Test]
        public void VerifyEvaluateWithOneIteration()
        {
            const double timeDelta = 0.01;
            const double valueDelta = 0.0001;

			FurnessData data = FurnessDataFactory.CreateFurnessDataForTestSetZeroDemand();
			var target = new FurnessEvaluator(data);

            // 1 iteration 
			Assert.AreEqual(1333, target.Evaluate(1, 1, Variances.StandardDeviation), valueDelta);
            Assert.AreEqual(20.6086, target.Data.TotalProduction(), valueDelta);
            Assert.AreEqual(17, target.Data.TotalProductionDemand(), valueDelta);
            Assert.AreEqual(21, target.Data.TotalProducerResources(), valueDelta);
            Assert.AreEqual(1, target.InnerIteration);
            target.Data.ResourceMatrix().DebugWriteMatrix();
            target.Data.ProductionMatrix().DebugWriteMatrix();
            Assert.AreEqual(2.3478, target.Data.ResourceMatrix()[0][0], timeDelta);
            Assert.AreEqual(3.6522, target.Data.ResourceMatrix()[0][1], timeDelta);
            Assert.AreEqual(0.0, target.Data.ResourceMatrix()[0][2], timeDelta);
            Assert.AreEqual(2.7391, target.Data.ResourceMatrix()[1][0], timeDelta);
            Assert.AreEqual(4.2609, target.Data.ResourceMatrix()[1][1], timeDelta);
            Assert.AreEqual(0.0, target.Data.ResourceMatrix()[1][2], timeDelta);
            Assert.AreEqual(0.0, target.Data.ResourceMatrix()[2][0], timeDelta);
            Assert.AreEqual(0.0, target.Data.ResourceMatrix()[2][1], timeDelta);
            Assert.AreEqual(8.0, target.Data.ResourceMatrix()[2][2], timeDelta);
        }

        [Test]
        public void VerifyEvaluateWithTwoIterations()
        {
            const double timeDelta = 0.01;
            const double valueDelta = 0.0001;
			
			FurnessData data = FurnessDataFactory.CreateFurnessDataForTestSetZeroDemand();
			var target = new FurnessEvaluator(data);
			
			Assert.AreEqual(1333, target.Evaluate(1, 2, Variances.StandardDeviation), valueDelta);
            Assert.AreEqual(2, target.InnerIteration);
            target.Data.ResourceMatrix().DebugWriteMatrix();
            target.Data.ProductionMatrix().DebugWriteMatrix();
            Assert.AreEqual(2.2766, target.Data.ResourceMatrix()[0][0], timeDelta);
            Assert.AreEqual(3.7234, target.Data.ResourceMatrix()[0][1], timeDelta);
            Assert.AreEqual(0.0, target.Data.ResourceMatrix()[0][2], timeDelta);
            Assert.AreEqual(2.6560, target.Data.ResourceMatrix()[1][0], timeDelta);
            Assert.AreEqual(4.3440, target.Data.ResourceMatrix()[1][1], timeDelta);
            Assert.AreEqual(0.0, target.Data.ResourceMatrix()[1][2], timeDelta);
            Assert.AreEqual(0.0, target.Data.ResourceMatrix()[2][0], timeDelta);
            Assert.AreEqual(0.0, target.Data.ResourceMatrix()[2][1], timeDelta);
            Assert.AreEqual(8.0, target.Data.ResourceMatrix()[2][2], timeDelta);
        }

        [Test]
        public void VerifyEvaluateWithFinalResult()
        {
            const double valueDelta = 0.0001;

			FurnessData data = FurnessDataFactory.CreateFurnessDataForTestSetZeroDemand();
			var target = new FurnessEvaluator(data);

			Assert.AreEqual(1333, target.Evaluate(1, 7, Variances.StandardDeviation), valueDelta);
            Assert.AreEqual(3, target.InnerIteration);
        }
    }
}
