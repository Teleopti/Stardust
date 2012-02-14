using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class SkillDataTest
    {
      
        private ServiceAgreement target;
        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = new ServiceAgreement();
            Percent percent = new Percent(0.123);
            double seconds = 1234;
            ServiceLevel sl = new ServiceLevel(percent, seconds);

            target.ServiceLevel = sl;
            target.MinOccupancy = new Percent(0.10);
            target.MaxOccupancy = new Percent(0.8);
        }

        [Test]
        public void CanSetAndGetProperties()
        {
            Percent percent = new Percent(0.1223);
            double seconds = 1234;
            ServiceLevel sl = new ServiceLevel(percent, seconds);

            Percent minOccupancy = new Percent(0.10);
            Percent maxOccupancy = new Percent(0.80);
            
            target.ServiceLevel = sl;
            target.MinOccupancy = minOccupancy;
            target.MaxOccupancy = maxOccupancy;

            Assert.AreEqual(sl,target.ServiceLevel);
            Assert.AreEqual(minOccupancy, target.MinOccupancy);
            Assert.AreEqual(maxOccupancy, target.MaxOccupancy);
        }

        [Test]
        public void VerifyEqualsWork()
        {
            Percent percent = new Percent(0.1223);
            double seconds = 1234;
            ServiceLevel sl = new ServiceLevel(percent, seconds);

            Percent minOccupancy = new Percent(0.10);
            Percent maxOccupancy = new Percent(0.80);

            target.ServiceLevel = sl;
            target.MinOccupancy = minOccupancy;
            target.MaxOccupancy = maxOccupancy;

            ServiceAgreement serviceAgreement = new ServiceAgreement();
            serviceAgreement.MinOccupancy = minOccupancy;
            serviceAgreement.MaxOccupancy = maxOccupancy;
            serviceAgreement.ServiceLevel = sl;

            Assert.IsTrue(target.Equals(serviceAgreement));
            Assert.IsFalse(new ServiceAgreement().Equals(null));
            Assert.AreEqual(target, target);
            Assert.IsFalse(new ServiceAgreement().Equals(3));
        }

        [Test]
        public void VerifyGetHashCodeWorks()
        {
            IDictionary<ServiceAgreement, int> dic = new Dictionary<ServiceAgreement, int>();
            dic[target] = 5;
      
            Assert.AreEqual(5, dic[target]);
        }

        [Test]
        public void VerifyOverloadedOperatorsWork()
        {
            Percent percent = new Percent(0.1245234223);
            double seconds = 1232342344;
            ServiceLevel sl = new ServiceLevel(percent, seconds);

            Percent minOccupancy = new Percent(0.10);
            Percent maxOccupancy = new Percent(0.80);

            ServiceAgreement serviceData1 = new ServiceAgreement();
            serviceData1.ServiceLevel = sl;
            serviceData1.MinOccupancy = minOccupancy;
            serviceData1.MaxOccupancy = maxOccupancy;

            ServiceAgreement serviceData2 = new ServiceAgreement();
            serviceData2.ServiceLevel = sl;
            serviceData2.MinOccupancy = minOccupancy;
            serviceData2.MaxOccupancy = maxOccupancy;

            Assert.IsTrue(serviceData1 == serviceData2);
            Assert.IsTrue(target != serviceData1);
        }
    }
}
