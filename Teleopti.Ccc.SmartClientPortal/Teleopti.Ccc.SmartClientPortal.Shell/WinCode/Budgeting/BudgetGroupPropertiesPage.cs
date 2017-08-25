using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting
{
    public class BudgetGroupPropertiesPage : AbstractPropertyPages<IBudgetGroup>
    {
        private readonly IBudgetGroup _budgetGroup;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public BudgetGroupPropertiesPage(IBudgetGroup budgetGroup, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
            : base(budgetGroup,repositoryFactory,unitOfWorkFactory)
        {
            _budgetGroup = budgetGroup;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public override IBudgetGroup CreateNewRoot()
        {
            using (AbstractWizardPages<IBudgetGroup> budgetGroupWzardPage = new BudgetGroupWizardPage(_budgetGroup,RepositoryFactory,_unitOfWorkFactory))
            {
                return budgetGroupWzardPage.CreateNewRoot();
            }
        }
        public override string Name => String.Empty;

	    public override string WindowText => UserTexts.Resources.Properties;

	    public override IRepository<IBudgetGroup> RepositoryObject => RepositoryFactory.CreateBudgetGroupRepository(UnitOfWork);
    }
}
