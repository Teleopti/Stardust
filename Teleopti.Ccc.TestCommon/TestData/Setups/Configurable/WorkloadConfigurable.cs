using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class WorkloadConfigurable : IDataSetup
	{
		public string SkillName { get; set; }
		public string QueueSourceName { get; set; }
		public string WorkloadName { get; set; }
		public bool Open24Hours { get; set; }
		public TimePeriod OpenHourPeriod { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var skill = new SkillRepository(currentUnitOfWork).LoadAll().Single(x => x.Name.Equals(SkillName));
			var wl = new Workload(skill) {Name = WorkloadName};
			if (QueueSourceName != null)
			{
				var qs = new QueueSourceRepository(currentUnitOfWork).LoadAll().Single(x => x.Name.Equals(QueueSourceName));
				wl.AddQueueSource(qs);
			}
			foreach (var dayTemplate in wl.TemplateWeekCollection)
			{
				if(Open24Hours)
					dayTemplate.Value.MakeOpen24Hours();
				else
				{
				    dayTemplate.Value.ChangeOpenHours(new List<TimePeriod>(){OpenHourPeriod});
				}
			}
			new WorkloadRepository(currentUnitOfWork).Add(wl);
		}

	}
}