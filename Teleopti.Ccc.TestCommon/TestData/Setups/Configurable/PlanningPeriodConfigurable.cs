using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PlanningPeriodConfigurable : IDataSetup
	{

		public PlanningPeriod Period;
		
		public DateTime Date { get; set; }
		
		public void Apply (ICurrentUnitOfWork currentUnitOfWork)
		{

			var period = new AggregatedSchedulePeriod()
			{
				DateFrom = Date,
				Culture = CultureInfo.CurrentCulture.LCID,
				PeriodType = SchedulePeriodType.Month,
				Number = 1,
				Priority = 1
			};
			
			var planningPeriodSuggestions = new PlanningPeriodSuggestions (new MutableNow (Date),new[] {period});
			
			Period = new PlanningPeriod (planningPeriodSuggestions);
			

			new PlanningPeriodRepository(currentUnitOfWork).Add(Period);
		}



	}
}