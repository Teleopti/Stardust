using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class SkillStaffSegmentTest
    {
        private ISkillStaffSegment target;

        [SetUp]
        public void Setup()
        {
            target = new SkillStaffSegment(3);
        }


        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(3, target.ForecastedDistributedDemand);
        }
    }

}
