using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.WinCode.Budgeting.Models;
using Teleopti.Ccc.WinCode.Budgeting.Presenters;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Budgeting
{
	public class BudgetGroupNavigatorDataService : IBudgetNavigatorDataService
	{
		private readonly IBudgetGroupRepository _budgetGroupRepository;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public BudgetGroupNavigatorDataService(IBudgetGroupRepository budgetGroupRepository, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_budgetGroupRepository = budgetGroupRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public BudgetGroupRootModel GetBudgetRootModels()
		{
            using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var root = new BudgetGroupRootModel();

                var budgetGroups = _budgetGroupRepository.LoadAll();
                foreach (var budgetGroup in budgetGroups)
                {
                    var groupModel = new BudgetGroupModel(budgetGroup);
                    foreach (var skill in budgetGroup.SkillCollection)
                    {
                        if (skill is IMultisiteSkill) continue;
                        groupModel.SkillModels.Add(new SkillModel(skill));
                    }
                    root.BudgetGroups.Add(groupModel);
                }
                return root;
            }
		}

		public void DeleteBudgetGroup(IBudgetGroup budgetGroup)
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				_budgetGroupRepository.Remove(budgetGroup);
				uow.PersistAll();
			}
		}

		public IBudgetGroup LoadBudgetGroup(IBudgetGroup budgetGroup)
		{
			if (budgetGroup == null) throw new ArgumentNullException("budgetGroup");
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
                var bg = _budgetGroupRepository.Get(budgetGroup.Id.GetValueOrDefault(Guid.Empty));
                if (!LazyLoadingManager.IsInitialized(bg.CustomShrinkages))
                {
                    LazyLoadingManager.Initialize(bg.CustomShrinkages);
                    bg.CustomShrinkages.ForEach(LazyLoadingManager.Initialize);
                }
                if (!LazyLoadingManager.IsInitialized(bg.CustomEfficiencyShrinkages))
                {
                    LazyLoadingManager.Initialize(bg.CustomEfficiencyShrinkages);
                    bg.CustomEfficiencyShrinkages.ForEach(LazyLoadingManager.Initialize);
                }
			    return bg;
			}
		}
	}
}