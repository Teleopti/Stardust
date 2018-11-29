using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingInformation
	{
		public SchedulingInformation(DateOnlyPeriod period, IEnumerable<Guid> personIds)
		{
			PersonIds = personIds;
			Period = period;
		}
		
		public IEnumerable<Guid> PersonIds { get; }
		public DateOnlyPeriod Period { get; }
	}
}