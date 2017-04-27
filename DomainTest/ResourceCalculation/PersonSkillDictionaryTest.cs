using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class PersonSkillDictionaryTest
    {
        private KeyedSkillResourceDictionary _target;
        private ISkill _skill1;
        private ISkill _skill2;
        private DoubleGuidCombinationKey _person1;
        private DoubleGuidCombinationKey _person2;
        private DoubleGuidCombinationKey _person3;

        [SetUp]
        public void Setup()
        {
            _target = new KeyedSkillResourceDictionary();
            _skill1 = SkillFactory.CreateSkill("s1");
            _skill2 = SkillFactory.CreateSkill("s2");

	        var person1Id = Guid.NewGuid();
	        var person2Id = Guid.NewGuid();
	        var person3Id = Guid.NewGuid();
	        _person1 = new DoubleGuidCombinationKey(new[] {person1Id}, new[] { person1Id });
	        _person2 = new DoubleGuidCombinationKey(new[] {person2Id}, new[] { person2Id });
	        _person3 = new DoubleGuidCombinationKey(new[] {person3Id}, new[] { person3Id });
	        _target.Add(_person1, new Dictionary<ISkill, double> { { _skill1, 0 }, { _skill2, 0 } });
	        _target.Add(_person2, new Dictionary<ISkill, double> { { _skill2, 0 } });
	        _target.Add(_person3, new Dictionary<ISkill, double> { { _skill2, 0 }, { _skill1, 0 } });
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
