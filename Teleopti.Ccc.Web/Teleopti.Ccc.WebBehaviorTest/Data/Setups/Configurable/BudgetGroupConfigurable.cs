using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
    public class BudgetGroupConfigurable : IUserSetup
    {
		public string Name { get; set; }
		public string Absence { get; set; }

	    public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
	    {
			var budgetGroupRepository = new BudgetGroupRepository(uow);
			var budgetGroup = new BudgetGroup() { Name = Name, TimeZone = user.PermissionInformation.DefaultTimeZone()};
			
			if (!string.IsNullOrEmpty(Absence))
			{
				var absenceRepository = new AbsenceRepository(uow);
				var absence = absenceRepository.LoadAll().First(a => a.Name == Absence);
				var shrinkage = new CustomShrinkage("shrinkage");
				shrinkage.AddAbsence(absence);
				budgetGroup.AddCustomShrinkage(shrinkage);
			}
			budgetGroupRepository.Add(budgetGroup);

			uow.PersistAll();
	    }
    }
}