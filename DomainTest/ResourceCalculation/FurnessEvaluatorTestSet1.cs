using NUnit.Framework;
using Teleopti.Ccc.Domain.Calculation;
using Teleopti.Ccc.Secrets.Furness;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class FurnessEvaluatorTestSet1
    {
        [Test]
        public void VerifyConstructor()
		{
			FurnessData data = FurnessDataFactory.CreateFurnessDataForTestSet1();
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

			FurnessData data = FurnessDataFactory.CreateFurnessDataForTestSet1();
			var target = new FurnessEvaluator(data);

            // 1 iteration 
            Assert.AreEqual(0.0425, target.Evaluate(1, 1, Variances.StandardDeviation), valueDelta);
            Assert.AreEqual(26.6986, target.Data.TotalProduction(), valueDelta);
            Assert.AreEqual(25, target.Data.TotalProductionDemand(), valueDelta);
            Assert.AreEqual(21, target.Data.TotalProducerResources(), valueDelta);
            Assert.AreEqual(1, target.InnerIteration);
            target.Data.ResourceMatrix().DebugWriteMatrix();
            target.Data.ProductionMatrix().DebugWriteMatrix();
            Assert.AreEqual(1.82, target.Data.ResourceMatrix()[0][0], timeDelta);
            Assert.AreEqual(1.89, target.Data.ResourceMatrix()[0][1], timeDelta);
            Assert.AreEqual(2.27, target.Data.ResourceMatrix()[0][2], timeDelta);
            Assert.AreEqual(3.43, target.Data.ResourceMatrix()[1][0], timeDelta);
            Assert.AreEqual(3.56, target.Data.ResourceMatrix()[1][1], timeDelta);
            Assert.AreEqual(0.0, target.Data.ResourceMatrix()[1][2], timeDelta);
            Assert.AreEqual(0.0, target.Data.ResourceMatrix()[2][0], timeDelta);
            Assert.AreEqual(3.63, target.Data.ResourceMatrix()[2][1], timeDelta);
            Assert.AreEqual(4.36, target.Data.ResourceMatrix()[2][2], timeDelta);

        }

        [Test]
        public void VerifyEvaluateWithTwoIterations()
        {
            const double timeDelta = 0.01;
            const double valueDelta = 0.0001;

			FurnessData data = FurnessDataFactory.CreateFurnessDataForTestSet1();
			var target = new FurnessEvaluator(data);

			Assert.AreEqual(0.0368, target.Evaluate(1, 2, Variances.StandardDeviation), valueDelta);
            Assert.AreEqual(2, target.InnerIteration);
            target.Data.ResourceMatrix().DebugWriteMatrix();
            target.Data.ProductionMatrix().DebugWriteMatrix();
            Assert.AreEqual(1.83, target.Data.ResourceMatrix()[0][0], timeDelta);
            Assert.AreEqual(1.96, target.Data.ResourceMatrix()[0][1], timeDelta);
            Assert.AreEqual(2.20, target.Data.ResourceMatrix()[0][2], timeDelta);
            Assert.AreEqual(3.37, target.Data.ResourceMatrix()[1][0], timeDelta);
            Assert.AreEqual(3.62, target.Data.ResourceMatrix()[1][1], timeDelta);
            Assert.AreEqual(0.0, target.Data.ResourceMatrix()[1][2], timeDelta);
            Assert.AreEqual(0.0, target.Data.ResourceMatrix()[2][0], timeDelta);
            Assert.AreEqual(3.76, target.Data.ResourceMatrix()[2][1], timeDelta);
            Assert.AreEqual(4.23, target.Data.ResourceMatrix()[2][2], timeDelta);

        }

        [Test]
        public void VerifyEvaluateIterationsTillStabilization()
		{
			FurnessData data = FurnessDataFactory.CreateFurnessDataForTestSet1();
			var target = new FurnessEvaluator(data);

	        target.Evaluate(1, 100, Variances.StandardDeviation);
            Assert.AreEqual(14, target.InnerIteration);
        }
    }
}
