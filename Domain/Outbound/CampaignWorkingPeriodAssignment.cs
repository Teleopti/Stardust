using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Domain.Outbound
{
	public class CampaignWorkingPeriodAssignment : AggregateEntity
	{
		public virtual DayOfWeek WeekdayIndex { get; set; }
		
	}
}
