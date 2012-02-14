using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class SchedulingStateHolderAllSkillExtractorTest
    {
        private SchedulingStateHolderAllSkillExtractor _target;
        private MockRepository _mock;
        private ISchedulingResultStateHolder _stateHolder;
        private IList<ISkill> _skillList;
        private ISkill _skill1;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _stateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
            _skillList = new List<ISkill>();
            _skill1 = SkillFactory.CreateSkill("skill1", SkillTypeFactory.CreateSkillType(), 15);
            _skillList.Add(_skill1);
            _target = new SchedulingStateHolderAllSkillExtractor(_stateHolder);
        }

        [Test]
        public void VerifyExtractsStateHolderAllSkills()
        {
            int skillNumber = _skillList.Count;
            using (_mock.Record())
            {
                Expect.Call(_stateHolder.Skills)
                    .Return(_skillList);
            }
            using (_mock.Playback())
            {
                IList<ISkill> result = _target.ExtractSkills().ToList();
                Assert.AreEqual(skillNumber, result.Count);
            }
        }
    }
}
