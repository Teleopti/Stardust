using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PlanningPeriodConfigurable : IDataSetup
	{

		public PlanningPeriod Period;
		
		public DateTime Date { get; set; }

		public string AgentGroupName { get; set; }
		
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
			IAgentGroup agentGroup = null;
			if (AgentGroupName != null)
				agentGroup = new AgentGroupRepository(currentUnitOfWork).LoadAll().FirstOrDefault(a => a.Name == AgentGroupName);
			
			var planningPeriodSuggestions = new PlanningPeriodSuggestions (new MutableNow (Date),new[] {period});
			
			Period = new PlanningPeriod (planningPeriodSuggestions, agentGroup);
			

			new PlanningPeriodRepository(currentUnitOfWork).Add(Period);
		}



	}
}