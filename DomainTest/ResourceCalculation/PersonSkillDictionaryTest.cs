using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class PersonSkillDictionaryTest
    {
        private KeyedSkillResourceDictionary _target;
        private ISkill _skill1;
        private ISkill _skill2;
        private string _person1;
        private string _person2;
        private string _person3;

        [SetUp]
        public void Setup()
        {
            _target = new KeyedSkillResourceDictionary();
            _skill1 = SkillFactory.CreateSkill("s1");
            _skill2 = SkillFactory.CreateSkill("s2");
            _person1 = "Person1";
            _person2 = "Person2";
            _person3 = "Person3";
            Dictionary<ISkill, double> skillDic = new Dictionary<ISkill, double>();
            skillDic.Add(_skill1, 0);
            skillDic.Add(_skill2, 0);
            _target.Add(_person1, skillDic);
            skillDic = new Dictionary<ISkill, double>();
            skillDic.Add(_skill2, 0);
            _target.Add(_person2, skillDic);
            skillDic = new Dictionary<ISkill, double>();
            skillDic.Add(_skill2, 0);
            skillDic.Add(_skill1, 0);
            _target.Add(_person3, skillDic);
        }

        [Test]
        public void VerifyConstructorOverload1()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyConstructorOverload2()
        {
            _target = new KeyedSkillResourceDictionaryTestClass(null, new StreamingContext());
            Assert.IsNotNull(_target);
        }

	    [Test]
        public void VerifyPersonKey()
        {
            Assert.AreEqual(1,_target[_person2].Values.Count);
        }

        [Test]
        public void VerifyCorrectSkill()
        {
            Assert.IsTrue(_target[_person1].ContainsKey(_skill1));
            Assert.IsTrue(_target[_person1].ContainsKey(_skill2));
            Assert.IsFalse(_target[_person2].ContainsKey(_skill1));
        }

        [Test]
        public void CanGetSkillCombination()
        {
            SkillCollectionKey key = _target.SkillCombination(_person1);
            Assert.AreEqual(2, key.SkillCollection.Count());
        }

        [Test]
        public void CanGetUniqueSkillCombinations()
        {
            ICollection<SkillCollectionKey> result = _target.UniqueSkillCombinations();
            Assert.AreEqual(2, result.Count);
        }

		private class KeyedSkillResourceDictionaryTestClass : KeyedSkillResourceDictionary
		{

			/// <summary>
			/// Initializes a new instance of the <see cref="KeyedSkillDictionaryTestClass"/> class.
			/// </summary>
			/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo"/> object containing the information required to serialize the <see cref="T:System.Collections.Generic.Dictionary`2"/>.</param>
			/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext"/> structure containing the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.Dictionary`2"/>.</param>
			public KeyedSkillResourceDictionaryTestClass(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}
    }
}
