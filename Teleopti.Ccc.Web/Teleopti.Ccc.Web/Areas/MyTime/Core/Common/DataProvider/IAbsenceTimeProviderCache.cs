using System.Collections.Generic;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IAbsenceTimeProviderCache
	{
		void Setup(IScenario scenario, DateOnlyPeriod period, IBudgetGroup budgetGroup);
		IEnumerable<PayloadWorkTime> Get();
		void Add(IEnumerable<PayloadWorkTime> absenceTime);
		string GetConfigValue();
	}
}