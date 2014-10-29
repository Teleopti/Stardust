using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IQuickForecasterWorkload
	{
		void Execute(IWorkload workload, DateOnlyPeriod historicalPeriod, DateOnlyPeriod futurePeriod, IEnumerable<ISkillDay> skillDays);
	}
}