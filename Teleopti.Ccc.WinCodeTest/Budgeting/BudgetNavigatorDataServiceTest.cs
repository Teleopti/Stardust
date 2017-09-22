using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
    [TestFixture]
    public class BudgetNavigatorDataServiceTest
    {
        private IBudgetNavigatorDataService target;
        private IUnitOfWorkFactory uowFactory;
        private MockRepository mock;
        private IBudgetGroupRepository budgetRepository;

        [SetUp]
        public void Setup()
        {
            mock = new MockRepository();
            uowFactory = mock.DynamicMock<IUnitOfWorkFactory>();
            budgetRepository = mock.StrictMock<IBudgetGroupRepository>();

            target = new BudgetGroupNavigatorDataService(budgetRepository, uowFactory);
        }

        [Test]
        public void ShouldGetBudgetRootModels()
        {
            var budgetGroup = new BudgetGroup {Name = "BG"};
            var skill = SkillFactory.CreateSkill("Skill");
            budgetGroup.AddSkill(skill);
            var multisiteSkill = SkillFactory.CreateMultisiteSkill("MultiSkill");
            budgetGroup.AddSkill(multisiteSkill);
            using (mock.Record())
            {
                Expect.Call(budgetRepository.LoadAll()).Return(new List<IBudgetGroup>{ budgetGroup });
            }

            using (mock.Playback())
            {
                var models = target.GetBudgetRootModels();

                Assert.AreEqual(1, models.BudgetGroups[0].SkillModels.Count);

                Assert.AreEqual(budgetGroup, models.BudgetGroups[0].ContainedEntity);
                Assert.AreEqual(skill, models.BudgetGroups[0].SkillModels[0].ContainedEntity);

                Assert.AreEqual(budgetGroup.Name, models.BudgetGroups[0].DisplayName);
                Assert.AreEqual(skill.Name, models.BudgetGroups[0].SkillModels[0].DisplayName);
            }
        }

        [Test]
        public void CanDelete()
        {
            var budgetGroup = new BudgetGroup { Name = "BG" };
            var uow = mock.StrictMock<IUnitOfWork>();
            using (mock.Record())
            {
                Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(uow);
                Expect.Call(uow.PersistAll()).Return(new List<IRootChangeInfo>() );
                Expect.Call(()=>budgetRepository.Remove(budgetGroup));
                Expect.Call(()=>uow.Dispose());
            }

            using (mock.Playback())
            {
                target.DeleteBudgetGroup(budgetGroup);
            }
        }

		[Test]
		public void ShouldLoadBudgetGroup()
		{
			IBudgetGroup budgetGroup = new BudgetGroup { Name = "BG" };
			budgetGroup.SetId(Guid.NewGuid());
			var budgetGroup1 = new BudgetGroup { Name = "BG1" };
	
			using (mock.Record())
			{
				Expect.Call(budgetRepository.Get(budgetGroup.Id.Value)).Return(budgetGroup1);
			}

			using (mock.Playback())
			{
				Assert.AreEqual(budgetGroup1,target.LoadBudgetGroup(budgetGroup));
                Assert.IsTrue(LazyLoadingManager.IsInitialized(budgetGroup.CustomShrinkages));
                Assert.IsTrue(LazyLoadingManager.IsInitialized(budgetGroup.CustomEfficiencyShrinkages));
			}
		}

        [Test]
        public void ShouldThrowOnLoadBudgetGroupGivenNull()
        {
	        Assert.Throws<ArgumentNullException>(() => target.LoadBudgetGroup(null));
        }
    }
}
