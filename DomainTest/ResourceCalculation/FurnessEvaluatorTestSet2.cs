using NUnit.Framework;
using Teleopti.Ccc.Domain.Calculation;
using Teleopti.Ccc.Secrets.Furness;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class FurnessEvaluatorTestSet2
    {
        [Test]
        public void VerifyConstructor()
        {
			FurnessData data = FurnessDataFactory.CreateFurnessDataForTestSet2();
			var target = new FurnessEvaluator(data);
            
			Assert.IsNotNull(target);
            Assert.AreEqual(2, target.Data.ProductTypes);
            Assert.AreEqual(3, target.Data.Producers);
        }

        [Test]
        public void VerifyEvaluateWithOneIteration()
        {
            const double timeDelta = 0.01;
            const double valueDelta = 0.0001;

			FurnessData data = FurnessDataFactory.CreateFurnessDataForTestSet2();
			var target = new FurnessEvaluator(data);

            // 1 iteration 
			Assert.AreEqual(0.6082, target.Evaluate(1, 1, Variances.StandardDeviation), valueDelta);
            Assert.AreEqual(200, target.Data.TotalProduction(), valueDelta);
            Assert.AreEqual(1500, target.Data.TotalProductionDemand(), valueDelta);
            Assert.AreEqual(200, target.Data.TotalProducerResources(), valueDelta);
            Assert.AreEqual(1, target.InnerIteration);
            target.Data.ResourceMatrix().DebugWriteMatrix();
            Assert.AreEqual(15.0, target.Data.ResourceMatrix()[0, 0], timeDelta);
            Assert.AreEqual(45.0, target.Data.ResourceMatrix()[0, 1], timeDelta);
            Assert.AreEqual(40.0, target.Data.ResourceMatrix()[1, 0], timeDelta);
            Assert.AreEqual(0.0, target.Data.ResourceMatrix()[1, 1], timeDelta);
            Assert.AreEqual(25.0, target.Data.ResourceMatrix()[2, 0], timeDelta);
            Assert.AreEqual(75.0, target.Data.ResourceMatrix()[2, 1], timeDelta);

        }

        [Test]
        public void VerifyEvaluateWithTwoIterations()
        {
            const double timeDelta = 0.01;
            const double valueDelta = 0.0001;
			
			FurnessData data = FurnessDataFactory.CreateFurnessDataForTestSet2();
			var target = new FurnessEvaluator(data);

			Assert.AreEqual(0.6109, target.Evaluate(1, 2, Variances.StandardDeviation), valueDelta);
            Assert.AreEqual(2, target.InnerIteration);
            target.Data.ResourceMatrix().DebugWriteMatrix();
            Assert.AreEqual(12.0, target.Data.ResourceMatrix()[0, 0], timeDelta);
            Assert.AreEqual(48.0, target.Data.ResourceMatrix()[0, 1], timeDelta);
            Assert.AreEqual(40.0, target.Data.ResourceMatrix()[1, 0], timeDelta);
            Assert.AreEqual(0.0, target.Data.ResourceMatrix()[1, 1], timeDelta);
            Assert.AreEqual(20.0, target.Data.ResourceMatrix()[2, 0], timeDelta);
            Assert.AreEqual(80.0, target.Data.ResourceMatrix()[2, 1], timeDelta);

        }

	    [Test]
	    public void VerifyEvaluateIterationsTillStabilization()
	    {
		    FurnessData data = FurnessDataFactory.CreateFurnessDataForTestSet2();
		    var target = new FurnessEvaluator(data);

		    target.Evaluate(1, 100, Variances.StandardDeviation);
		    Assert.AreEqual(20, target.InnerIteration);
	    }
    }
}
