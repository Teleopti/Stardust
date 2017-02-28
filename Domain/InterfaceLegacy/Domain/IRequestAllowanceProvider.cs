using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IRequestAllowanceProvider
	{
		IList<IBudgetAbsenceAllowanceDetail> GetBudgetAbsenceAllowanceDetails(DateOnlyPeriod period, IBudgetGroup selectedBudgetGroup, IEnumerable<IAbsence> absencesInBudgetGroup);
	}
}
