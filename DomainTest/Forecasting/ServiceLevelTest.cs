using NUnit.Framework;
using Teleopti.Ccc.DomainTest.Helper;
using System;
using System.Collections.Generic;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class ServiceLevelTest
    {
        private ServiceLevel target;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = new ServiceLevel(new Percent(0.96), 123);
        }

        [Test]
        public void CanSetAndGetProperties()
        {
            Percent percent = new Percent(0.1234);
            double seconds = 2322;

            target.Percent = percent;
            target.Seconds = seconds;

            Assert.AreEqual(target.Percent,percent);
            Assert.AreEqual(target.Seconds, seconds);
        }

        [Test]
        public void ConstructorWorks()
        {
            Percent percent = new Percent(0.123);
            double seconds = 1234;
            target = new ServiceLevel(percent, seconds);
            Assert.AreEqual(target.Percent, percent);
            Assert.AreEqual(target.Seconds, seconds);
        }

        /// <summary>
        /// Verifies the service level above one hundred percent gives exception.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-07
        /// </remarks>
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyServiceLevelAboveOneHundredPercentGivesException()
        {
            Percent percent = new Percent(1.01);
            
            target.Percent = percent;
        }

        /// <summary>
        /// Verifies the service level above one hundred percent gives exception in constructor.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-07
        /// </remarks>
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyServiceLevelAboveOneHundredPercentGivesExceptionInConstructor()
        {
            Percent percent = new Percent(1.01);

            target = new ServiceLevel(percent,target.Seconds);
        }

        [Test]
        public void VerifyProtectedConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType()));
        }

        /// <summary>
        /// Verifies the clone works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-21
        /// </remarks>
        [Test]
        public void VerifyCloneWorks()
        {
            ServiceLevel clone = (ServiceLevel)target.Clone();
            Assert.AreEqual(target, clone);
            Assert.AreNotSame(target, clone);
        }

        /// <summary>
        /// Verifies the equals and not equals.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-21
        /// </remarks>
        [Test]
        public void VerifyEqualsAndNotEquals()
        {
            ServiceLevel sl1 = new ServiceLevel(new Percent(0.3), 23);
            ServiceLevel sl2 = new ServiceLevel(new Percent(0.3), 23);

            Assert.IsTrue(sl1 != target);
            Assert.IsTrue(sl1 == sl2);
            Assert.IsFalse(sl1 != sl2);
        }

        /// <summary>
        /// Verifies the get hash code.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-21
        /// </remarks>
        [Test]
        public void VerifyGetHashCode()
        {
            IDictionary<ServiceLevel,int> dic = new Dictionary<ServiceLevel,int>();
            dic.Add(target, 7);

            Assert.AreEqual(7, dic[target]);
        }

        /// <summary>
        /// Verifies the overloaded equals.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-21
        /// </remarks>
        [Test]
        public void VerifyOverloadedEquals()
        {
            object obj = new object();

            Assert.IsFalse(target.Equals(obj));
        }
    }
}
