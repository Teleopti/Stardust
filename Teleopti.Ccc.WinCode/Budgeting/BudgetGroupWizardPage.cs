﻿using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Budgeting
{
    public class BudgetGroupWizardPage : AbstractWizardPages<IBudgetGroup>
    {
        public BudgetGroupWizardPage(IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
            : base(repositoryFactory, unitOfWorkFactory)
        {
        }

        public BudgetGroupWizardPage(IBudgetGroup budgetGroup, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
            : base(budgetGroup,repositoryFactory,unitOfWorkFactory)
        {
        }

        public override IBudgetGroup CreateNewRoot()
        {
            IBudgetGroup budgetGroup = new BudgetGroup();
            budgetGroup.Name = UserTexts.Resources.LessThanBudgetGroupNameGreaterThan;
            budgetGroup.TrySetDaysPerYear(365);
            budgetGroup.TimeZone = TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone;

            return budgetGroup;
        }

        public override string Name
        {
            get
            {
                var planningGroup = AggregateRootObject;
                return planningGroup.Name;
            }
        }

        public override string WindowText
        {
            get { return UserTexts.Resources.NewBudgetGroup; }
        }

        public override IRepository<IBudgetGroup> RepositoryObject
        {
            get
            {
                return RepositoryFactory.CreateBudgetGroupRepository(UnitOfWork);
            }
        }
    }
}
