using NUnit.Framework;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{

    [TestFixture]
    public class FurnessEvaluatorCollectionTest
    {
        private FurnessEvaluatorCollection<FurnessEvaluator> _target;
        private FurnessEvaluator _evaluator1;
        private FurnessEvaluator _evaluator2;

        [SetUp]
        public void Setup()
        {
            _evaluator1 = new FurnessEvaluator(3, 3);
            _evaluator1.Data.ProductionDemand()[0] = 9;
            _evaluator1.Data.ProductionDemand()[1] = 8;
            _evaluator1.Data.ProductionDemand()[2] = 8;

            _evaluator1.Data.ProducerResources()[0] = 6;
            _evaluator1.Data.ProducerResources()[1] = 7;
            _evaluator1.Data.ProducerResources()[2] = 8;

            _evaluator1.Data.ProductivityMatrix()[0, 0] = 1.5;
            _evaluator1.Data.ProductivityMatrix()[0, 1] = 1.5;
            _evaluator1.Data.ProductivityMatrix()[0, 2] = 1.0;

            _evaluator1.Data.ProductivityMatrix()[1, 0] = 2.0;
            _evaluator1.Data.ProductivityMatrix()[1, 1] = 0.5;
            _evaluator1.Data.ProductivityMatrix()[1, 2] = 0.0;

            _evaluator1.Data.ProductivityMatrix()[2, 0] = 0.0;
            _evaluator1.Data.ProductivityMatrix()[2, 1] = 1.0;
            _evaluator1.Data.ProductivityMatrix()[2, 2] = 1.5;

            _evaluator1.Data.ResourceMatrix()[0, 0] = 1;
            _evaluator1.Data.ResourceMatrix()[0, 1] = 1;
            _evaluator1.Data.ResourceMatrix()[0, 2] = 1;

            _evaluator1.Data.ResourceMatrix()[1, 0] = 1;
            _evaluator1.Data.ResourceMatrix()[1, 1] = 1;
            _evaluator1.Data.ResourceMatrix()[1, 2] = 0;

            _evaluator1.Data.ResourceMatrix()[2, 0] = 0;
            _evaluator1.Data.ResourceMatrix()[2, 1] = 1;
            _evaluator1.Data.ResourceMatrix()[2, 2] = 1;

            _evaluator2 = new FurnessEvaluator(3, 3);
            _evaluator2.Data.ProductionDemand()[0] = 9;
            _evaluator2.Data.ProductionDemand()[1] = 8;
            _evaluator2.Data.ProductionDemand()[2] = 8;

            _evaluator2.Data.ProducerResources()[0] = 6;
            _evaluator2.Data.ProducerResources()[1] = 7;
            _evaluator2.Data.ProducerResources()[2] = 8;

            _evaluator2.Data.ProductivityMatrix()[0, 0] = 1.5;
            _evaluator2.Data.ProductivityMatrix()[0, 1] = 1.5;
            _evaluator2.Data.ProductivityMatrix()[0, 2] = 1.0;

            _evaluator2.Data.ProductivityMatrix()[1, 0] = 2.0;
            _evaluator2.Data.ProductivityMatrix()[1, 1] = 0.5;
            _evaluator2.Data.ProductivityMatrix()[1, 2] = 0.0;

            _evaluator2.Data.ProductivityMatrix()[2, 0] = 0.0;
            _evaluator2.Data.ProductivityMatrix()[2, 1] = 1.0;
            _evaluator2.Data.ProductivityMatrix()[2, 2] = 1.5;

            _evaluator2.Data.ResourceMatrix()[0, 0] = 1;
            _evaluator2.Data.ResourceMatrix()[0, 1] = 1;
            _evaluator2.Data.ResourceMatrix()[0, 2] = 1;

            _evaluator2.Data.ResourceMatrix()[1, 0] = 1;
            _evaluator2.Data.ResourceMatrix()[1, 1] = 1;
            _evaluator2.Data.ResourceMatrix()[1, 2] = 0;

            _evaluator2.Data.ResourceMatrix()[2, 0] = 0;
            _evaluator2.Data.ResourceMatrix()[2, 1] = 1;
            _evaluator2.Data.ResourceMatrix()[2, 2] = 1;

            _target = new FurnessEvaluatorCollection<FurnessEvaluator>();
            _target.Add(_evaluator1);
            _target.Add(_evaluator2);
        }

        [TearDown]
        public void Teardown()
        {
            _target = null;
        }

        [Test]
        public void VerifyOptimize()
        {
            double valueDelta = 0.0001;

            double quotient = _target.Evaluate();
            Assert.AreEqual(2, _target.OuterIteration);
            Assert.AreEqual(1.06469, quotient, valueDelta);

            quotient = _target.Evaluate(1, 1);
            Assert.AreEqual(1, _target.OuterIteration);
            Assert.AreEqual(1.06473, quotient, valueDelta);

                //// 1 iteration 
                //Assert.AreEqual(0.0425, _target.Evaluate(1, 1), valueDelta);
                //Assert.AreEqual(26.6986, _target.TotalProduction(), valueDelta);
                //Assert.AreEqual(25, _target.TotalProductionDemand(), valueDelta);
                //Assert.AreEqual(21, _target.TotalProducerResources(), valueDelta);
                //Dump(_target.ResourceMatrix);
                //Assert.AreEqual(1.82, _target.ResourceMatrix[0, 0], timeDelta);
                //Assert.AreEqual(1.89, _target.ResourceMatrix[0, 1], timeDelta);
                //Assert.AreEqual(2.27, _target.ResourceMatrix[0, 2], timeDelta);
                //Assert.AreEqual(3.43, _target.ResourceMatrix[1, 0], timeDelta);
                //Assert.AreEqual(3.56, _target.ResourceMatrix[1, 1], timeDelta);
                //Assert.AreEqual(0.0, _target.ResourceMatrix[1, 2], timeDelta);
                //Assert.AreEqual(0.0, _target.ResourceMatrix[2, 0], timeDelta);
                //Assert.AreEqual(3.63, _target.ResourceMatrix[2, 1], timeDelta);
                //Assert.AreEqual(4.36, _target.ResourceMatrix[2, 2], timeDelta);

        }
    }
}
