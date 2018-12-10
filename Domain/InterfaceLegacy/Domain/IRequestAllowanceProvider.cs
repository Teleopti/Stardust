using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IRequestAllowanceProvider
	{
		IList<IBudgetAbsenceAllowanceDetail> GetBudgetAbsenceAllowanceDetails(DateOnlyPeriod period, IBudgetGroup selectedBudgetGroup, IEnumerable<IAbsence> absencesInBudgetGroup);
	}
}
