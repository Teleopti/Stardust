using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    /// <summary>
    /// Tests for SkillPersonData
    /// </summary>
    [TestFixture]
    public class SkillPersonDataTest
    {
        private SkillPersonData target;
        private int minAgents = 3;
        private int maxAgents = 8;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = new SkillPersonData(3,8);
        }

        [Test]
        public void VerifyEmptyConstructor()
        {
            target = new SkillPersonData();
        }

        [Test]
        public void CanSetAndGetProperties()
        {
            int newMinAgents = minAgents - 1;
            int newMaxAgents = maxAgents + 4;
            target.MinimumPersons = newMinAgents;
            target.MaximumPersons = newMaxAgents;
            Assert.AreEqual(newMinAgents, target.MinimumPersons);
            Assert.AreEqual(newMaxAgents, target.MaximumPersons);
        }

        [Test]
        public void VerifyEqualsWork()
        {
            target.MinimumPersons = minAgents;
            target.MaximumPersons = maxAgents;
            
            SkillPersonData skillPersonData = new SkillPersonData(3,8);
            
            Assert.IsTrue(target.Equals(skillPersonData));
            Assert.IsFalse(new SkillPersonData().Equals(null));
            Assert.AreEqual(target, target);
            Assert.IsFalse(new SkillPersonData().Equals(3));
        }

        [Test]
        public void VerifyGetHashCodeWorks()
        {
            IDictionary<SkillPersonData, int> dic = new Dictionary<SkillPersonData, int>();
            dic[target] = 5;

            Assert.AreEqual(5, dic[target]);
        }

        [Test]
        public void VerifyOverloadedOperatorsWork()
        {
            SkillPersonData skillPersonData1 = new SkillPersonData(32,99);
            SkillPersonData skillPersonData2 = new SkillPersonData(32,99);

            Assert.IsTrue(skillPersonData1 == skillPersonData2);
            Assert.IsTrue(target != skillPersonData1);
        }

        /// <summary>
        /// Verifies the is valid works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-17
        /// </remarks>
        [Test]
        public void VerifyIsValidWorks()
        {
            SkillPersonData skillPersonData1 = new SkillPersonData(1, 1); //Valid
            SkillPersonData skillPersonData2 = new SkillPersonData(1, 0); //Valid
            SkillPersonData skillPersonData3 = new SkillPersonData(0, 1); //Valid
            SkillPersonData skillPersonData4 = new SkillPersonData(0, 0); //Valid
            SkillPersonData skillPersonData5 = new SkillPersonData(2, 1); //Invalid
            SkillPersonData skillPersonData6 = new SkillPersonData(-1, 1); //Invalid
            SkillPersonData skillPersonData7 = new SkillPersonData(3, -1); //Invalid

            Assert.IsTrue(target.IsValid);
            Assert.IsTrue(skillPersonData1.IsValid);
            Assert.IsTrue(skillPersonData2.IsValid);
            Assert.IsTrue(skillPersonData3.IsValid);
            Assert.IsTrue(skillPersonData4.IsValid);
            Assert.IsFalse(skillPersonData5.IsValid);
            Assert.IsFalse(skillPersonData6.IsValid);
            Assert.IsFalse(skillPersonData7.IsValid);
        }
    }
}
