using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Budgeting
{
    [TestFixture]
    public class BudgetGroupTest
    {
        private IBudgetGroup target;

        [SetUp]
        public void Setup()
        {
            target = new BudgetGroup();
        }

        [Test]
        public void CanAddSkill()
        {
            var skill = SkillFactory.CreateSkill("Skill");
            target.AddSkill(skill);
            Assert.AreEqual(target.SkillCollection.First(),skill);
        }

        [Test]
        public void CanRemoveAllSkills()
        {
            var skill = SkillFactory.CreateSkill("Skill");
            var skill2 = SkillFactory.CreateSkill("Skill2");
            target.AddSkill(skill);
            target.AddSkill(skill2);
            Assert.AreEqual(2, target.SkillCollection.Count());
            target.RemoveAllSkills();
            Assert.AreEqual(0,target.SkillCollection.Count());
        }

        [Test]
        public void Properties()
        {
            const string description = "budget";
            var timezone = new CccTimeZoneInfo(TimeZoneInfo.GetSystemTimeZones()[1]);
            
            target.Name = description;
            target.TimeZone = timezone;
 
            Assert.AreEqual(description, target.Name);
            Assert.AreEqual(timezone.Id, target.TimeZone.Id);
        }

        [Test]
        public void ShouldAddShrinkage()
        {
            ICustomShrinkage customShrinkage = new CustomShrinkage("Vacation");
            target.AddCustomShrinkage(customShrinkage);
            Assert.AreEqual(customShrinkage,target.CustomShrinkages.Single());
        }

        [Test]
        public void ShouldRemoveShrinkage()
        {
            ICustomShrinkage customShrinkage = new CustomShrinkage("Vacation");
            target.AddCustomShrinkage(customShrinkage);
            target.RemoveCustomShrinkage(customShrinkage);
            Assert.AreEqual(0, target.CustomShrinkages.Count());
        }

        [Test]
        public void ShouldAddEfficiencyShrinkage()
        {
            ICustomEfficiencyShrinkage customShrinkage = new CustomEfficiencyShrinkage("Coffee");
            target.AddCustomEfficiencyShrinkage(customShrinkage);
            Assert.AreEqual(customShrinkage, target.CustomEfficiencyShrinkages.Single());
        }

        [Test]
        public void ShouldRemoveEfficiencyShrinkage()
        {
            ICustomEfficiencyShrinkage customShrinkage = new CustomEfficiencyShrinkage("Coffee");
            target.AddCustomEfficiencyShrinkage(customShrinkage);
            target.RemoveCustomEfficiencyShrinkage(customShrinkage);
            Assert.AreEqual(0, target.CustomEfficiencyShrinkages.Count());
        }

        [Test]
        public void ShouldReturnTrueIfCustomShrinkageExists()
        {
            Guid id = Guid.NewGuid();
            ICustomShrinkage customShrinkage = new CustomShrinkage("Vacation");
            customShrinkage.SetId(id);
            target.AddCustomShrinkage(customShrinkage);
            Assert.IsTrue(target.IsCustomShrinkage(id));
        }

        [Test]
        public void ShouldReturnFalseIfCustomShrinkageNotExists()
        {
            Guid id = Guid.NewGuid();
            ICustomShrinkage customShrinkage = new CustomShrinkage("Vacation");
            customShrinkage.SetId(id);
            target.AddCustomShrinkage(customShrinkage);
            Assert.IsFalse(target.IsCustomShrinkage(Guid.NewGuid()));
        }

        [Test]
        public void ShouldReturnTrueIfCustomEfficiencyShrinkageExists()
        {
            Guid id = Guid.NewGuid();
            ICustomEfficiencyShrinkage customShrinkage = new CustomEfficiencyShrinkage("Coffee");
            customShrinkage.SetId(id);
            target.AddCustomEfficiencyShrinkage(customShrinkage);
            Assert.IsTrue(target.IsCustomEfficiencyShrinkage(id));
        }

        [Test]
        public void ShouldReturnFalseIfCustomEfficiencyShrinkageNotExists()
        {
            Guid id = Guid.NewGuid();
            ICustomEfficiencyShrinkage customShrinkage = new CustomEfficiencyShrinkage("Coffee");
            customShrinkage.SetId(id);
            target.AddCustomEfficiencyShrinkage(customShrinkage);
            Assert.IsFalse(target.IsCustomEfficiencyShrinkage(Guid.NewGuid()));
        }

        [Test]
        public void ValidateDaysPerYearToMuch()
        {
            target.TrySetDaysPerYear(399);

            target.DaysPerYear
                .Should().Be.EqualTo(DaysInYear());
        }

        [Test]
        public void ValidateDaysPerYearToLess()
        {
            target.TrySetDaysPerYear(-14);

            target.DaysPerYear
                .Should().Be.EqualTo(1);
        }

        [Test]
        public void ValidateDaysNormal()
        {
            target.TrySetDaysPerYear(244);
            target.DaysPerYear
                .Should().Be.EqualTo(244);
        }

        [Test]
        public void ShouldDelete()
        {
            var deleteTarget = (IDeleteTag)target;
            deleteTarget.SetDeleted();
            deleteTarget.IsDeleted.Should().Be.True();
        }

        [Test]
        public void ShouldUpdateCustomShrinkageWithNewValue()
        {
            Guid id = Guid.NewGuid();
            ICustomShrinkage customShrinkage = new CustomShrinkage("Vacation", false);
            customShrinkage.SetId(id);
            target.AddCustomShrinkage(customShrinkage);
            target.UpdateCustomShrinkage(id, new CustomShrinkage("New Vacation", true));
            Assert.AreEqual("New Vacation", target.GetShrinkage(id).ShrinkageName);
            Assert.IsTrue(target.GetShrinkage(id).IncludedInAllowance);
        }
        
        [Test]
        public void ShouldUpdateCustomEfficiencyShrinkageWithNewValue()
        {
            Guid id = Guid.NewGuid();
            ICustomEfficiencyShrinkage customEfficiencyShrinkage = new CustomEfficiencyShrinkage("Coffee", false);
            customEfficiencyShrinkage.SetId(id);
            target.AddCustomEfficiencyShrinkage(customEfficiencyShrinkage);
            target.UpdateCustomEfficiencyShrinkage(id, new CustomEfficiencyShrinkage("Fika", true));
            Assert.AreEqual("Fika", target.GetEfficiencyShrinkage(id).ShrinkageName);
            Assert.IsTrue(target.GetEfficiencyShrinkage(id).IncludedInAllowance);
        }

        private int DaysInYear()
        {
            return DateTime.IsLeapYear(DateTime.Now.Year) ? 366 : 365;
        }
    }
}