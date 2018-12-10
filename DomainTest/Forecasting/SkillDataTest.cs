using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class SkillDataTest
    {
        private ServiceAgreement target;

        [SetUp]
        public void Setup()
        {
            Percent percent = new Percent(0.123);
            double seconds = 1234;
            ServiceLevel sl = new ServiceLevel(percent, seconds);
			target = new ServiceAgreement(sl,new Percent(0.1), new Percent(0.8));
        }
		
        [Test]
        public void VerifyEqualsWork()
        {
            Percent percent = new Percent(0.123);
            double seconds = 1234;

            ServiceLevel sl = new ServiceLevel(percent, seconds);
            Percent minOccupancy = new Percent(0.10);
            Percent maxOccupancy = new Percent(0.80);
			
            ServiceAgreement serviceAgreement = new ServiceAgreement(sl,minOccupancy,maxOccupancy);

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

	        ServiceAgreement serviceData1 = new ServiceAgreement(sl, minOccupancy, maxOccupancy);
            ServiceAgreement serviceData2 = new ServiceAgreement(sl, minOccupancy, maxOccupancy);

			Assert.IsTrue(serviceData1 == serviceData2);
            Assert.IsTrue(target != serviceData1);
        }
    }
}
