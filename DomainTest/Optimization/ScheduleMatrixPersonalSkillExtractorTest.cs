using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class ScheduleMatrixPersonalSkillExtractorTest
    {
        private ScheduleMatrixPersonalSkillExtractor _target;
        private MockRepository _mock;
        private IScheduleMatrixPro _scheduleMatrix;
        private IScheduleDayPro _scheduleDayPro;
        private IPerson _person;
        private IList<ISkill> _skillList;
        private ISkill _skill1;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _scheduleMatrix = _mock.StrictMock<IScheduleMatrixPro>();
            _scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
            _skillList = new List<ISkill>();
            _skill1 = SkillFactory.CreateSkill("skill1", SkillTypeFactory.CreateSkillTypePhone(), 15);
            _skillList.Add(_skill1);
            _person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2010, 1, 1), _skillList);
            _target = new ScheduleMatrixPersonalSkillExtractor(_scheduleMatrix, new PersonalSkillsProvider());
        }

        [Test]
        public void VerifyPersonWithOneSkill()
        {
            int personSkillNumber = _skillList.Count;
            using(_mock.Record())
            {
                Expect.Call(_scheduleMatrix.Person).Return(_person);
                Expect.Call(_scheduleMatrix.EffectivePeriodDays)
                    .Return(new [] { _scheduleDayPro });
                Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2010, 4, 1)).Repeat.Any();
            }
            using(_mock.Playback())
            {
                IList<ISkill> result = _target.ExtractSkills().ToList();
                Assert.AreEqual(personSkillNumber, result.Count);
            }
        }
    }
}
