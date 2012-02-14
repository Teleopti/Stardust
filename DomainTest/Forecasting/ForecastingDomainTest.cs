using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    /// <summary>
    /// Tests for ForecastingDomain
    /// </summary>
    [TestFixture]
    public class ForecastingDomainTest
    {
        private ISkill _skill;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _skill = null;
        }

        /// <summary>
        /// Verifies the can build forecast source structure.
        /// </summary>
        [Test]
        public void VerifyCanBuildWorkloadStructure()
        {
            _skill = SkillFactory.CreateSkillWithWorkloadAndSources();

            Assert.IsNotNull(_skill);
        }
    }
}