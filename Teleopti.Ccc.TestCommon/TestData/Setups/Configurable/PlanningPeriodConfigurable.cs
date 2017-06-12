using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PlanningPeriodConfigurable : IDataSetup
	{
		public PlanningPeriod Period;
		
		public DateTime Date { get; set; }

		public string PlanningGroupName { get; set; }
		
		public void Apply (ICurrentUnitOfWork currentUnitOfWork)
		{

			var period = new AggregatedSchedulePeriod
			{
				DateFrom = Date,
				Culture = CultureInfo.CurrentCulture.LCID,
				PeriodType = SchedulePeriodType.Month,
				Number = 1,
				Priority = 1
			};
			IPlanningGroup planningGroup = null;
			if (PlanningGroupName != null)
				planningGroup = new PlanningGroupRepository(currentUnitOfWork).LoadAll().FirstOrDefault(a => a.Name == PlanningGroupName);
			
			var planningPeriodSuggestions = new PlanningPeriodSuggestions (new MutableNow (Date),new[] {period});
			
			Period = new PlanningPeriod (planningPeriodSuggestions, planningGroup);
			

			new PlanningPeriodRepository(currentUnitOfWork).Add(Period);
		}
	}
}