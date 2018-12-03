using NUnit.Framework;
using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class ServiceLevelTest
    {
        private ServiceLevel target;
		
        [SetUp]
        public void Setup()
        {
            target = new ServiceLevel(new Percent(0.96), 123);
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
		
        [Test]
        public void VerifyServiceLevelAboveOneHundredPercentGivesExceptionInConstructor()
        {
            Percent percent = new Percent(1.01);

			Assert.Throws<ArgumentOutOfRangeException>(() => target = new ServiceLevel(percent,target.Seconds));
        }
		
        [Test]
        public void VerifyCloneWorks()
        {
            ServiceLevel clone = (ServiceLevel)target.Clone();
            Assert.AreEqual(target, clone);
            Assert.AreNotSame(target, clone);
        }
		
        [Test]
        public void VerifyEqualsAndNotEquals()
        {
            ServiceLevel sl1 = new ServiceLevel(new Percent(0.3), 23);
            ServiceLevel sl2 = new ServiceLevel(new Percent(0.3), 23);

            Assert.IsTrue(sl1 != target);
            Assert.IsTrue(sl1 == sl2);
            Assert.IsFalse(sl1 != sl2);
        }
		
        [Test]
        public void VerifyGetHashCode()
        {
            IDictionary<ServiceLevel,int> dic = new Dictionary<ServiceLevel,int>();
            dic.Add(target, 7);

            Assert.AreEqual(7, dic[target]);
        }
		
        [Test]
        public void VerifyOverloadedEquals()
        {
            object obj = new object();

            Assert.IsFalse(target.Equals(obj));
        }
    }
}
