using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class ScheduleDayRestrictor : IScheduleDayRestrictor
	{
		public IList<IScheduleDay> RemoveScheduleDayEndingTooLate(IList<IScheduleDay> scheduleParts, DateTime givenDate)
		{
			return scheduleParts.Where(s => s.DateOnlyAsPeriod.Period().EndDateTime <= givenDate).ToList();
		}
	}
}