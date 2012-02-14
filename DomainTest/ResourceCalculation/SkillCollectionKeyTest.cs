using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class SkillCollectionKeyTest
    {
        private SkillCollectionKey _target;
        private ISkill _s1;
        private ISkill _s2;
        private ISkill _s3;

        [SetUp]
        public void Setup()
        {
            _s1 = SkillFactory.CreateSkill("s1");
            _s2 = SkillFactory.CreateSkill("s2");
            _s3 = SkillFactory.CreateSkill("s3");
            _target = new SkillCollectionKey(new List<ISkill>{ _s1, _s2 });
            _target.VirtualSkillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(new DateTimePeriod(), new Task(), new ServiceAgreement());
            _target.SumOccWeight = 200;
            _target.SumTraff = 2;
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual(2, _target.SkillCollection.Count());
            Assert.IsNotNull(_target.VirtualSkillStaffPeriod);
            Assert.AreEqual(2, _target.SumTraff);
            Assert.AreEqual(200, _target.SumOccWeight);
        }

        [Test]
        public void VerifyGetHashCode()
        {
            SkillCollectionKey sameHashKey = new SkillCollectionKey(new List<ISkill>() {_s1, _s2 });
            SkillCollectionKey notSameHashKey = new SkillCollectionKey(new List<ISkill>() { _s1, _s3 });

            Assert.AreEqual(_target.GetHashCode(), sameHashKey.GetHashCode());
            Assert.AreNotEqual(_target.GetHashCode(), notSameHashKey.GetHashCode());

        }

        [Test]
        public void VerifyGetHashCode2()
        {
            SkillCollectionKey sameHashKey = new SkillCollectionKey(new List<ISkill>() { _s2, _s1 });
            Assert.AreEqual(_target.GetHashCode(), sameHashKey.GetHashCode());
        }

        [Test]
        public void VerifyEqual()
        {
            SkillCollectionKey equalKey = new SkillCollectionKey(new List<ISkill>() { _s1, _s2 });
            SkillCollectionKey notEqualKey = new SkillCollectionKey(new List<ISkill>() { _s1, _s3 });

            Assert.IsTrue(_target.Equals(equalKey));
            Assert.IsFalse(_target.Equals(notEqualKey));
            Assert.IsTrue(_target == equalKey);
            Assert.IsTrue(_target != notEqualKey);
        }

        [Test]
        public void VerifyEqual2()
        {
            SkillCollectionKey equalKey = new SkillCollectionKey(new List<ISkill>() { _s2, _s1 });
            Assert.IsTrue(_target.Equals(equalKey));
        }

        [Test]
        public void VerifyToStringWorks()
        {
            Assert.AreEqual("s1s2", _target.ToString());
        }
        
        [Test]
        public void VerifyEqualsObjectWorks()
        {
            object equalKey = new SkillCollectionKey(new List<ISkill>() { _s1, _s2 });
            Assert.IsTrue(_target.Equals(equalKey));
            Assert.IsFalse(_target.Equals(7));
        }

       
    }
}
