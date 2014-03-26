using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("LongRunning")]
    public class BudgetGroupRepositoryTest : RepositoryTest<IBudgetGroup>
    {
        private string description;
        private TimeZoneInfo timeZone;
        private ISkill _skill;

        protected override void ConcreteSetup()
        {
            _skill = SkillFactory.CreateSkill("skill");
            var skillType = SkillTypeFactory.CreateSkillType();
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

        protected override Repository<IBudgetGroup> TestRepository(IUnitOfWork unitOfWork)
        {
            return new BudgetGroupRepository(unitOfWork);
        }

        [Test]
        public void ShouldCreateRepositoryWithUnitOfWorkFactory()
        {
            IBudgetGroupRepository budgetGroupRepository = new BudgetGroupRepository(UnitOfWorkFactory.Current);
            Assert.IsNotNull(budgetGroupRepository);
        }
    }
}
