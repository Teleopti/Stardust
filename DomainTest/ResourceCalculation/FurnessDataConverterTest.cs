using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Secrets.Furness;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class FurnessDataConverterTest
    {
        private FurnessDataConverter _target;

        DividedActivityData _activityData;

        // skill efficiency
        private ISkill _phoneA;
        private ISkill _phoneB;

        // p3 is ignorant, see excel test case
        private string _person1;
        private string _person2;
        private string _person4;


        [SetUp]
        public void Setup()
        {
            _activityData = new DividedActivityData();

            // skill efficiency
            _phoneA = SkillFactory.CreateSkill("PhoneA");
            _phoneB = SkillFactory.CreateSkill("PhoneB");

            // p3 is ignorant, see excel test case
            _person1 = "Person1";
            _person2 = "Person2";
            _person4 = "Person4";

            var p1s = new Dictionary<ISkill, double> {{_phoneA, 2}, {_phoneB, 1}};

            var p2s = new Dictionary<ISkill, double> {{_phoneA, 1}, {_phoneB, 1}};

            var p4s = new Dictionary<ISkill, double> {{_phoneB, 1}};

            _activityData.KeyedSkillResourceEfficiencies.Add(_person1, p1s);
            _activityData.KeyedSkillResourceEfficiencies.Add(_person2, p2s);
            _activityData.KeyedSkillResourceEfficiencies.Add(_person4, p4s);

            // resource matrix
            var p1r = new Dictionary<ISkill, double> {{_phoneA, 0.6666}, {_phoneB, 0.3333}};

            var p2r = new Dictionary<ISkill, double> {{_phoneA, 1}, {_phoneB, 1}};

            var p4r = new Dictionary<ISkill, double> {{_phoneB, 0.6666}};

            _activityData.WeightedRelativeKeyedSkillResourceResources.Add(_person1, p1r);
            _activityData.WeightedRelativeKeyedSkillResourceResources.Add(_person2, p2r);
            _activityData.WeightedRelativeKeyedSkillResourceResources.Add(_person4, p4r);

            // demands
            _activityData.TargetDemands.Add(_phoneA, 500d);
            _activityData.TargetDemands.Add(_phoneB, 1000d);

            // relative resources
            _activityData.RelativePersonResources.Add(_person1, 0.3333d);
            _activityData.RelativePersonResources.Add(_person2, 1d);
            _activityData.RelativePersonResources.Add(_person4, 0.6666d);

            // resources
            _activityData.PersonResources.Add(_person1, 5d);
            _activityData.PersonResources.Add(_person2, 15d);
            _activityData.PersonResources.Add(_person4, 10d);

            _target = new FurnessDataConverter(_activityData);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        //[Test]
        //public void VerifyProperties()
        //{
        //    Assert.AreSame(_personResources, _target.PersonResources);
        //    Assert.AreSame(_targetDemands, _target.TargetDemands);
        //    Assert.AreSame(_skillEfficiencyMatrix, _target.KeyedSkillResourceEfficiencies);
        //    Assert.AreSame(_resourceMatrix, _target.ResourceMatrix);
        //}

        [Test]
        public void VerifyConvertToFurnessData()
        {
            IFurnessData furnessData = _target.ConvertDividedActivityToFurnessData();
            Assert.IsNotNull(furnessData);

            Assert.AreEqual(_activityData.TargetDemands.Count, furnessData.ProductTypes);
            Assert.AreEqual(_activityData.TargetDemands[_phoneA], furnessData.ProductionDemand()[0]);
            Assert.AreEqual(_activityData.TargetDemands[_phoneB], furnessData.ProductionDemand()[1]);

            Assert.AreEqual(_activityData.PersonResources.Count, furnessData.Producers);
            Assert.AreEqual(_activityData.PersonResources[_person1], furnessData.ProducerResources()[0]);
            Assert.AreEqual(_activityData.PersonResources[_person2], furnessData.ProducerResources()[1]);
            Assert.AreEqual(_activityData.PersonResources[_person4], furnessData.ProducerResources()[2]);

            Assert.AreEqual(_activityData.WeightedRelativeKeyedSkillResourceResources[_person1][_phoneA], furnessData.ResourceMatrix()[0, 0]);
            Assert.AreEqual(_activityData.WeightedRelativeKeyedSkillResourceResources[_person1][_phoneB], furnessData.ResourceMatrix()[0, 1]);
            Assert.AreEqual(_activityData.WeightedRelativeKeyedSkillResourceResources[_person2][_phoneA], furnessData.ResourceMatrix()[1, 0]);
            Assert.AreEqual(_activityData.WeightedRelativeKeyedSkillResourceResources[_person2][_phoneB], furnessData.ResourceMatrix()[1, 1]);
            // this key pair does not exist, so the value must be 0
            Assert.AreEqual(0d, furnessData.ResourceMatrix()[2, 0]);
            Assert.AreEqual(_activityData.WeightedRelativeKeyedSkillResourceResources[_person4][_phoneB], furnessData.ResourceMatrix()[2, 1]);

            Assert.AreEqual(1d, furnessData.ProductivityMatrix()[0, 0]);
            Assert.AreEqual(_activityData.KeyedSkillResourceEfficiencies[_person1][_phoneB], furnessData.ProductivityMatrix()[0, 1]);
            Assert.AreEqual(_activityData.KeyedSkillResourceEfficiencies[_person2][_phoneA], furnessData.ProductivityMatrix()[1, 0]);
            Assert.AreEqual(_activityData.KeyedSkillResourceEfficiencies[_person2][_phoneB], furnessData.ProductivityMatrix()[1, 1]);
            // this key pair does not exist, so the value must be 0
            Assert.AreEqual(0d, furnessData.ProductivityMatrix()[2, 0]);

            Assert.AreEqual(_activityData.KeyedSkillResourceEfficiencies[_person4][_phoneB], furnessData.ProductivityMatrix()[2, 1]);

            Assert.AreEqual(1d, furnessData.ProductivityMatrix()[0, 0]);
            Assert.AreEqual(1d, furnessData.ProductivityMatrix()[0, 1]);
            Assert.AreEqual(1d, furnessData.ProductivityMatrix()[1, 0]);
            Assert.AreEqual(1d, furnessData.ProductivityMatrix()[1, 1]);
            // this key pair does not exist, so the value must be 0
            Assert.AreEqual(0d, furnessData.ProductivityMatrix()[2, 0]);
            Assert.AreEqual(1d, furnessData.ProductivityMatrix()[2, 1]);

        }

        [Test]
        public void VerifyConvertBackFurnessDataResult()
        {
            IFurnessData furnessData = _target.ConvertDividedActivityToFurnessData();
            for (int personIndex = 0;personIndex < furnessData.Producers;personIndex++)
            {
                for(int skillIndex = 0;skillIndex < furnessData.ProductTypes;skillIndex++)
                {
                    furnessData.ResourceMatrix()[personIndex, skillIndex] *= 2;
                    furnessData.ProductionMatrix()[personIndex, skillIndex] =
                        furnessData.ResourceMatrix()[personIndex, skillIndex]*
                        furnessData.ProductivityMatrix()[personIndex, skillIndex];
                }
            }

            _target.ConvertFurnessDataBackToActivity();

            Assert.AreEqual(_activityData.WeightedRelativeKeyedSkillResourceResources[_person1][_phoneA], furnessData.ResourceMatrix()[0, 0]);
            Assert.AreEqual(_activityData.WeightedRelativeKeyedSkillResourceResources[_person1][_phoneB], furnessData.ResourceMatrix()[0, 1]);
            Assert.AreEqual(_activityData.WeightedRelativeKeyedSkillResourceResources[_person2][_phoneA], furnessData.ResourceMatrix()[1, 0]);
            Assert.AreEqual(_activityData.WeightedRelativeKeyedSkillResourceResources[_person2][_phoneB], furnessData.ResourceMatrix()[1, 1]);
            Assert.AreEqual(_activityData.WeightedRelativeKeyedSkillResourceResources[_person4][_phoneB], furnessData.ResourceMatrix()[2, 1]);

        }
    }

}
