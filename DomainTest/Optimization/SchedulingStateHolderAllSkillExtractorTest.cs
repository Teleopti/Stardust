using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;

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
            _skill1 = SkillFactory.CreateSkill("skill1", SkillTypeFactory.CreateSkillTypePhone(), 15);
            _skillList.Add(_skill1);
	        var maxSeatSkill = SkillFactory.CreateSkill("hej", new SkillTypePhone(new Description(), ForecastSource.MaxSeatSkill), 15);
			_skillList.Add(maxSeatSkill);
            _target = new SchedulingStateHolderAllSkillExtractor(()=>_stateHolder);
        }

        [Test]
        public void VerifyExtractsStateHolderAllSkillsExceptMaxSeat()
        {

            using (_mock.Record())
            {
                Expect.Call(_stateHolder.VisibleSkills)
                    .Return(_skillList);
            }
            using (_mock.Playback())
            {
                IList<ISkill> result = _target.ExtractSkills().ToList();
                Assert.AreEqual(1, result.Count);
            }
        }

		[Test]
		public void MaxSeatSkillsShouldNotBeIncluded()
		{
			var skill2 = SkillFactory.CreateSiteSkill("maxSeat");
			_skillList.Add(skill2);

			using (_mock.Record())
			{
				Expect.Call(_stateHolder.VisibleSkills)
					.Return(_skillList);
			}
			using (_mock.Playback())
			{
				IList<ISkill> result = _target.ExtractSkills().ToList();
				Assert.AreEqual(1, result.Count);
				Assert.AreSame(_skill1, result[0]);
			}
      
      
		}
    }
}
