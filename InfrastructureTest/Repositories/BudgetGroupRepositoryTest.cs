using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("BucketB")]
    public class BudgetGroupRepositoryTest : RepositoryTest<IBudgetGroup>
    {
        private string description;
        private TimeZoneInfo timeZone;
        private ISkill _skill;

        protected override void ConcreteSetup()
        {
            _skill = SkillFactory.CreateSkill("skill");
            var skillType = SkillTypeFactory.CreateSkillTypePhone();
            PersistAndRemoveFromUnitOfWork(skillType);

            var activity = new Activity("a");
            PersistAndRemoveFromUnitOfWork(activity);

            var staffingThresholds = new StaffingThresholds(new Percent(0.1), new Percent(0.2), new Percent(0.3));
            var midnightBreakOffset = new TimeSpan(3, 0, 0);

            _skill.SkillType = skillType;
            _skill.Activity = activity;
            _skill.MidnightBreakOffset = midnightBreakOffset;
            _skill.StaffingThresholds = staffingThresholds;
            PersistAndRemoveFromUnitOfWork(_skill);
        }

        protected override IBudgetGroup CreateAggregateWithCorrectBusinessUnit()
        {
            description = "budget";
            timeZone = TimeZoneInfo.GetSystemTimeZones()[0];
            var budgetGroup = new BudgetGroup { Name = description, TimeZone = timeZone };
            budgetGroup.TrySetDaysPerYear(365);
            budgetGroup.AddCustomShrinkage(new CustomShrinkage("Vacation"));
            budgetGroup.AddCustomEfficiencyShrinkage(new CustomEfficiencyShrinkage("Coffee"));
            budgetGroup.AddSkill(_skill);
            return budgetGroup;
        }

        protected override void VerifyAggregateGraphProperties(IBudgetGroup loadedAggregateFromDatabase)
        {
            var budgetGroup = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(budgetGroup.DaysPerYear, loadedAggregateFromDatabase.DaysPerYear);
            Assert.AreEqual(budgetGroup.TimeZone.Id, loadedAggregateFromDatabase.TimeZone.Id);
            Assert.AreEqual(budgetGroup.Name, loadedAggregateFromDatabase.Name);
            Assert.AreEqual(budgetGroup.SkillCollection.First(), _skill);
            Assert.AreEqual(budgetGroup.CustomShrinkages.Count(),loadedAggregateFromDatabase.CustomShrinkages.Count());
            Assert.AreEqual(budgetGroup.CustomEfficiencyShrinkages.Count(),loadedAggregateFromDatabase.CustomEfficiencyShrinkages.Count());
        }

        protected override Repository<IBudgetGroup> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new BudgetGroupRepository(currentUnitOfWork);
        }
    }
}
