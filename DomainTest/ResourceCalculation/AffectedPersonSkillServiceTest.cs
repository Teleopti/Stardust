using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class AffectedPersonSkillServiceTest
    {
	    [Test]
	    public void CanGetValidSkills()
	    {
			ISkill skill = SkillFactory.CreateSkill("d");
		    var validSkills = new List<ISkill> {skill};
		    var target = new AffectedPersonSkillService(validSkills);

			IEnumerable<ISkill> valid = target.AffectedSkills;
		    Assert.AreEqual(1, valid.Count());
		    Assert.AreSame(skill, valid.First());
	    }
		
		[Test]
		public void AffectedSkillsShouldNotContainMaxSeatSkillOrNonBlendSkill()
		{
			var validSkills = new List<ISkill>
			{
				SkillFactory.CreateSkill("vanligt"),
				SkillFactory.CreateSiteSkill("site"),
				SkillFactory.CreateNonBlendSkill("non")
			};

			var target = new AffectedPersonSkillService(validSkills);
			Assert.AreEqual(1, target.AffectedSkills.Count());
			ISkill skill = target.AffectedSkills.FirstOrDefault();
			Assert.AreEqual("vanligt", skill.Name);
		}
    }
}
