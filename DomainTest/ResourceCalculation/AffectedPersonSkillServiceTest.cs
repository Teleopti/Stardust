using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class AffectedPersonSkillServiceTest
    {
        private IAffectedPersonSkillService _target;
        private IList<ISkill> _validSkills;

        [SetUp]
        public void Setup()
        {
            _validSkills = new List<ISkill>();
            _target = new AffectedPersonSkillService(_validSkills);
        }

	    [Test]
	    public void CanGetValidSkills()
	    {
		    ISkill skill = SkillFactory.CreateSkill("d");
		    _validSkills.Add(skill);
		    IEnumerable<ISkill> valid = _target.AffectedSkills;
		    Assert.AreEqual(1, valid.Count());
		    Assert.AreSame(skill, valid.First());
	    }


		[Test]
		public void AffectedSkillsShouldNotContainMaxSeatSkillOrNonBlendSkill()
		{
			_validSkills.Add(SkillFactory.CreateSkill("vanligt"));
			_validSkills.Add(SkillFactory.CreateSiteSkill("site"));
			_validSkills.Add(SkillFactory.CreateNonBlendSkill("non"));
			_target = new AffectedPersonSkillService(_validSkills);
			Assert.AreEqual(1, _target.AffectedSkills.Count());
			ISkill skill = _target.AffectedSkills.FirstOrDefault();
			Assert.AreEqual("vanligt", skill.Name);
		}
    }
}
