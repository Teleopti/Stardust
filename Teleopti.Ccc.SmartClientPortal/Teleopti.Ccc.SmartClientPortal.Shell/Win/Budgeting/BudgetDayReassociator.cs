using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.WinCode.Budgeting.Models;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Budgeting
{
	public class BudgetDayReassociator
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly BudgetGroupMainModel _budgetGroupMainModel;

		public BudgetDayReassociator(IUnitOfWorkFactory unitOfWorkFactory, BudgetGroupMainModel budgetGroupMainModel)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_budgetGroupMainModel = budgetGroupMainModel;
		}

		public void Reassociate()
		{
			var unitOfWork = _unitOfWorkFactory.CurrentUnitOfWork();
			unitOfWork.Reassociate(_budgetGroupMainModel.BudgetGroup);
			if (!LazyLoadingManager.IsInitialized(_budgetGroupMainModel.BudgetGroup))
				LazyLoadingManager.Initialize(_budgetGroupMainModel.BudgetGroup);
			unitOfWork.Reassociate(_budgetGroupMainModel.BudgetGroup.SkillCollection);
			foreach (ISkill skill in _budgetGroupMainModel.BudgetGroup.SkillCollection)
			{
				unitOfWork.Reassociate(skill.SkillType);
				if (!LazyLoadingManager.IsInitialized(skill.SkillType))
					LazyLoadingManager.Initialize(skill.SkillType);
				unitOfWork.Reassociate(skill.WorkloadCollection);
				if (!LazyLoadingManager.IsInitialized(skill.WorkloadCollection))
					LazyLoadingManager.Initialize(skill.WorkloadCollection);
				var multisiteSkill = skill as IMultisiteSkill;
				if (multisiteSkill != null)
				{
					unitOfWork.Reassociate(multisiteSkill);
					if (!LazyLoadingManager.IsInitialized(multisiteSkill.ChildSkills))
						LazyLoadingManager.Initialize(multisiteSkill.ChildSkills);
				}
			}
		}
	}
}