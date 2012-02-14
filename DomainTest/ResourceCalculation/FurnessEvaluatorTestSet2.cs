﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;
using System.Globalization;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{

    [TestFixture]
    public class FurnessEvaluatorTestSet2
    {
        private FurnessEvaluator _target;

        [SetUp]
        public void Setup()
        {
            FurnessData data = FurnessDataFactory.CreateFurnessDataForTestSet2();
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
            Assert.AreEqual(2, _target.Data.ProductTypes);
            Assert.AreEqual(3, _target.Data.Producers);
        }

        #endregion

        [Test]
        public void VerifyEvaluateWithOneIteration()
        {
            double timeDelta = 0.01;
            double valueDelta = 0.0001;

            // 1 iteration 
            Assert.AreEqual(0.6082, _target.Evaluate(1, 1), valueDelta);
            Assert.AreEqual(200, _target.Data.TotalProduction(), valueDelta);
            Assert.AreEqual(1500, _target.Data.TotalProductionDemand(), valueDelta);
            Assert.AreEqual(200, _target.Data.TotalProducerResources(), valueDelta);
            Assert.AreEqual(1, _target.InnerIteration);
            DebugWriteMatrix(_target.Data.ResourceMatrix());
            Assert.AreEqual(15.0, _target.Data.ResourceMatrix()[0, 0], timeDelta);
            Assert.AreEqual(45.0, _target.Data.ResourceMatrix()[0, 1], timeDelta);
            Assert.AreEqual(40.0, _target.Data.ResourceMatrix()[1, 0], timeDelta);
            Assert.AreEqual(0.0, _target.Data.ResourceMatrix()[1, 1], timeDelta);
            Assert.AreEqual(25.0, _target.Data.ResourceMatrix()[2, 0], timeDelta);
            Assert.AreEqual(75.0, _target.Data.ResourceMatrix()[2, 1], timeDelta);

        }

        [Test]
        public void VerifyEvaluateWithTwoIterations()
        {
            double timeDelta = 0.01;
            double valueDelta = 0.0001;

            Assert.AreEqual(0.6109, _target.Evaluate(1, 2), valueDelta);
            Assert.AreEqual(2, _target.InnerIteration);
            DebugWriteMatrix(_target.Data.ResourceMatrix());
            Assert.AreEqual(12.0, _target.Data.ResourceMatrix()[0, 0], timeDelta);
            Assert.AreEqual(48.0, _target.Data.ResourceMatrix()[0, 1], timeDelta);
            Assert.AreEqual(40.0, _target.Data.ResourceMatrix()[1, 0], timeDelta);
            Assert.AreEqual(0.0, _target.Data.ResourceMatrix()[1, 1], timeDelta);
            Assert.AreEqual(20.0, _target.Data.ResourceMatrix()[2, 0], timeDelta);
            Assert.AreEqual(80.0, _target.Data.ResourceMatrix()[2, 1], timeDelta);

        }

        [Test]
        public void VerifyEvaluateIterationsTillStabilization()
        {
            _target.Evaluate(1, 100);
            Assert.AreEqual(10, _target.InnerIteration);
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
