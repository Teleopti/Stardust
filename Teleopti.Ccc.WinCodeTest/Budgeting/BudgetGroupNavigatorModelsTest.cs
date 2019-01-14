using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
    [TestFixture]
    public class BudgetGroupNavigatorModelsTest
    {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void CanCreateSkillModel()
        {
            var skill = new Skill("MySkill", "Description", Color.Beige, 15, SkillTypeFactory.CreateSkillTypePhone()); 
            var skillModel = new SkillModel(skill);
            
            Assert.AreEqual(skill, skillModel.ContainedEntity);
            Assert.AreEqual(skill.Name, skillModel.DisplayName);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void CanCreateBudgetGroupModel()
        {
            var budegetGroup = new BudgetGroup {Name = "BG"};
            var budegetGroupModel = new BudgetGroupModel(budegetGroup);

            Assert.AreEqual(budegetGroup, budegetGroupModel.ContainedEntity);
            Assert.AreEqual(budegetGroup.Name, budegetGroupModel.DisplayName);
        }
    }
}
