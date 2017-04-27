using NUnit.Framework;
using Teleopti.Ccc.Domain.Calculation;
using Teleopti.Ccc.Secrets.Furness;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{

    [TestFixture]
    public class FurnessEvaluatorTestSet3
    {
        [Test]
        public void VerifyConstructor()
        {
			FurnessData data = FurnessDataFactory.CreateFurnessDataForTestSet3();
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
			
			FurnessData data = FurnessDataFactory.CreateFurnessDataForTestSet3();
			var target = new FurnessEvaluator(data);
            
			// 1 iteration 
			target.Evaluate(1, 1, Variances.StandardDeviation);
            Assert.AreEqual(15, target.Data.TotalProduction(), valueDelta);
            Assert.AreEqual(14.4d, target.Data.TotalProductionDemand(), valueDelta);
            Assert.AreEqual(15, target.Data.TotalProducerResources(), valueDelta);
            Assert.AreEqual(1, target.InnerIteration);
            target.Data.ResourceMatrix().DebugWriteMatrix();
            Assert.AreEqual(12.0, target.Data.ResourceMatrix()[0][0], timeDelta);
            Assert.AreEqual(0, target.Data.ResourceMatrix()[0][1], timeDelta);
            Assert.AreEqual(0, target.Data.ResourceMatrix()[0][2], timeDelta);

            Assert.AreEqual(2.14, target.Data.ResourceMatrix()[1][0], timeDelta);
            Assert.AreEqual(0.85, target.Data.ResourceMatrix()[1][1], timeDelta);
            Assert.AreEqual(0.0, target.Data.ResourceMatrix()[1][2], timeDelta);

            Assert.AreEqual(0, target.Data.ResourceMatrix()[2][0], timeDelta);
            Assert.AreEqual(0, target.Data.ResourceMatrix()[2][1], timeDelta);
            Assert.AreEqual(0, target.Data.ResourceMatrix()[2][2], timeDelta);
        }

        [Test]
        public void VerifyEvaluateWithTwoIterations()
        {
            const double timeDelta = 0.01;
			
			FurnessData data = FurnessDataFactory.CreateFurnessDataForTestSet3();
			var target = new FurnessEvaluator(data);

			target.Evaluate(1, 2, Variances.StandardDeviation);
            Assert.AreEqual(2, target.InnerIteration);
            target.Data.ResourceMatrix().DebugWriteMatrix();
            Assert.AreEqual(12.0, target.Data.ResourceMatrix()[0][0], timeDelta);
            Assert.AreEqual(0, target.Data.ResourceMatrix()[0][1], timeDelta);
            Assert.AreEqual(0, target.Data.ResourceMatrix()[0][2], timeDelta);

            Assert.AreEqual(1.29, target.Data.ResourceMatrix()[1][0], timeDelta);
            Assert.AreEqual(1.7, target.Data.ResourceMatrix()[1][1], timeDelta);
            Assert.AreEqual(0.0, target.Data.ResourceMatrix()[1][2], timeDelta);

            Assert.AreEqual(0, target.Data.ResourceMatrix()[2][0], timeDelta);
            Assert.AreEqual(0, target.Data.ResourceMatrix()[2][1], timeDelta);
            Assert.AreEqual(0, target.Data.ResourceMatrix()[2][2], timeDelta);
        }

	    [Test]
	    public void VerifyEvaluateIterationsTillStabilization()
	    {
		    FurnessData data = FurnessDataFactory.CreateFurnessDataForTestSet3();
		    var target = new FurnessEvaluator(data);

		    target.Evaluate(1, 100, Variances.StandardDeviation);
		    Assert.AreEqual(43, target.InnerIteration);
	    }
    }
}
