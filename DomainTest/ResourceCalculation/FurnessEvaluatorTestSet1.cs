using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Globalization;
using Teleopti.Ccc.Domain.Calculation;
using Teleopti.Ccc.Secrets.Furness;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{

    [TestFixture]
    public class FurnessEvaluatorTestSet1
    {
        private FurnessEvaluator _target;

        [SetUp]
        public void Setup()
        {
            FurnessData data = FurnessDataFactory.CreateFurnessDataForTestSet1();
            _target = new FurnessEvaluator(data);
        }

        [TearDown]
        public void Teardown()
        {
            _target = null;
        }

        #region Verify constructor

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual(3, _target.Data.ProductTypes);
            Assert.AreEqual(3, _target.Data.Producers);
        }

        #endregion

        [Test]
        public void VerifyEvaluateWithOneIteration()
        {
            double timeDelta = 0.01;
            double valueDelta = 0.0001;

            // 1 iteration 
            Assert.AreEqual(0.0425, _target.Evaluate(1, 1, Variances.StandardDeviation), valueDelta);
            Assert.AreEqual(26.6986, _target.Data.TotalProduction(), valueDelta);
            Assert.AreEqual(25, _target.Data.TotalProductionDemand(), valueDelta);
            Assert.AreEqual(21, _target.Data.TotalProducerResources(), valueDelta);
            Assert.AreEqual(1, _target.InnerIteration);
            DebugWriteMatrix(_target.Data.ResourceMatrix());
            DebugWriteMatrix(_target.Data.ProductionMatrix());
            Assert.AreEqual(1.82, _target.Data.ResourceMatrix()[0, 0], timeDelta);
            Assert.AreEqual(1.89, _target.Data.ResourceMatrix()[0, 1], timeDelta);
            Assert.AreEqual(2.27, _target.Data.ResourceMatrix()[0, 2], timeDelta);
            Assert.AreEqual(3.43, _target.Data.ResourceMatrix()[1, 0], timeDelta);
            Assert.AreEqual(3.56, _target.Data.ResourceMatrix()[1, 1], timeDelta);
            Assert.AreEqual(0.0, _target.Data.ResourceMatrix()[1, 2], timeDelta);
            Assert.AreEqual(0.0, _target.Data.ResourceMatrix()[2, 0], timeDelta);
            Assert.AreEqual(3.63, _target.Data.ResourceMatrix()[2, 1], timeDelta);
            Assert.AreEqual(4.36, _target.Data.ResourceMatrix()[2, 2], timeDelta);

        }

        [Test]
        public void VerifyEvaluateWithTwoIterations()
        {
            double timeDelta = 0.01;
            double valueDelta = 0.0001;

			Assert.AreEqual(0.0368, _target.Evaluate(1, 2, Variances.StandardDeviation), valueDelta);
            Assert.AreEqual(2, _target.InnerIteration);
            DebugWriteMatrix(_target.Data.ResourceMatrix());
            DebugWriteMatrix(_target.Data.ProductionMatrix());
            Assert.AreEqual(1.83, _target.Data.ResourceMatrix()[0, 0], timeDelta);
            Assert.AreEqual(1.96, _target.Data.ResourceMatrix()[0, 1], timeDelta);
            Assert.AreEqual(2.20, _target.Data.ResourceMatrix()[0, 2], timeDelta);
            Assert.AreEqual(3.37, _target.Data.ResourceMatrix()[1, 0], timeDelta);
            Assert.AreEqual(3.62, _target.Data.ResourceMatrix()[1, 1], timeDelta);
            Assert.AreEqual(0.0, _target.Data.ResourceMatrix()[1, 2], timeDelta);
            Assert.AreEqual(0.0, _target.Data.ResourceMatrix()[2, 0], timeDelta);
            Assert.AreEqual(3.76, _target.Data.ResourceMatrix()[2, 1], timeDelta);
            Assert.AreEqual(4.23, _target.Data.ResourceMatrix()[2, 2], timeDelta);

        }

        [Test]
        public void VerifyEvaluateIterationsTillStabilization()
        {
		  _target.Evaluate(1, 100, Variances.StandardDeviation);
            Assert.AreEqual(14, _target.InnerIteration);
        }

        private static void DebugWriteMatrix(double[,] a)
        {
            for (int iAgent = 0; iAgent <= a.GetUpperBound(0); iAgent++)
            {
                for (int jSkill = 0; jSkill <= a.GetUpperBound(1); jSkill++)
                {
                    Debug.Write(a[iAgent, jSkill].ToString("0.0000", CultureInfo.CurrentCulture) + "\t");
                }
                Debug.WriteLine("");
            }
            Debug.WriteLine("");
        } 
    }
}
