using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.ExportPages
{
    [TestFixture]
    public class DestinationSkillModelTest
    {
        private DestinationSkillModel _target;

        [Test]
        public void ShouldHaveAllProperties()
        {
            const string childName = "child";
            const string multisiteName = "MultisiteSkill";
	        var multisiteSkill = SkillFactory.CreateMultisiteSkill(multisiteName);
			var childSkill = new ChildSkill(childName, "desc", Color.Beige, multisiteSkill).WithId();
            var selected = new Skill("source", "desc", Color.Beige, 15, SkillTypeFactory.CreateSkillTypePhone());
            selected.SetId(Guid.NewGuid());
            var childSkillMappingModel = new ChildSkillMappingModel(Guid.NewGuid(),
                                                                    selected.Id.GetValueOrDefault(),
                                                                    "Bu", selected.Name);
            _target = new DestinationSkillModel(childSkill, new[] { childSkillMappingModel });
            
            Assert.That(_target.Skill, Is.EqualTo(childSkill));
            Assert.That(_target.ChildSkill, Is.EqualTo(childName));
            Assert.That(_target.ChildSkillMapping.ToArray()[0], Is.EqualTo(childSkillMappingModel));
            Assert.That(_target.ParentSkill, Is.EqualTo(multisiteName));
        }

        [Test]
        public void ChildSkillMappingIsNotEmpty()
        {
            const string childName = "child";
            const string multisiteName = "MultisiteSkill";
            var guid = Guid.NewGuid();
			var multisiteSkill = SkillFactory.CreateMultisiteSkill(multisiteName);
            var childSkill = new ChildSkill(childName, "desc", Color.Beige, multisiteSkill).WithId(guid);
            var selected = new Skill("source", "desc", Color.Beige, 15, SkillTypeFactory.CreateSkillTypePhone());
            selected.SetId(Guid.NewGuid());
            var childSkillMappingModel = new ChildSkillMappingModel(guid,
                                                                    selected.Id.GetValueOrDefault(),
                                                                    "Bu", selected.Name);

            _target = new DestinationSkillModel(childSkill, new[] { childSkillMappingModel });
            Assert.That(_target.TargetBu, Is.EqualTo("Bu"));
            Assert.That(_target.TargetSkill, Is.EqualTo(selected.Name));
        }

    }
}
