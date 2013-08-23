using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.SeatLimitations
{
    [TestFixture]
    public class SkillVisualLayerCollectionDictionaryTest
    {
        private SkillVisualLayerCollectionDictionary _target;
        private ISkill _skill1;
        private ISkill _skill2;
        private IVisualLayerCollection _visualLayerCollectionSite1;
        private IVisualLayerCollection _visualLayerCollectionSite2;
        private IPerson _person1;
        private IPerson _person2;

        [SetUp]
        public void Setup()
        {
            _target = new SkillVisualLayerCollectionDictionary();

        	_skill1 = SkillFactory.CreateSkill("");
			_skill2 = SkillFactory.CreateSkill("");

            _person1 = PersonFactory.CreatePerson("Person1");
            _person2 = PersonFactory.CreatePerson("Person2");

            _visualLayerCollectionSite1 = VisualLayerCollectionFactory.CreateForWorkShift(_person1, new TimeSpan(), new TimeSpan());
            _visualLayerCollectionSite2 = VisualLayerCollectionFactory.CreateForWorkShift(_person2, new TimeSpan(), new TimeSpan());

			_target.Add(_skill1, new List<IVisualLayerCollection> { _visualLayerCollectionSite1 });
			_target.Add(_skill2, new List<IVisualLayerCollection> { _visualLayerCollectionSite2 });

        }

        [Test]
        public void VerifyRightItemCanBeFound()
        {
			Assert.AreEqual(_target[_skill1][0], _visualLayerCollectionSite1);
			Assert.AreEqual(_target[_skill2][0], _visualLayerCollectionSite2);
        }


        [Test]
        public void VerifyConstructorOverload2()
        {
            _target = new SkillVisualLayerCollectionDictionary();
            Assert.IsNotNull(_target);
        }
    }
}
