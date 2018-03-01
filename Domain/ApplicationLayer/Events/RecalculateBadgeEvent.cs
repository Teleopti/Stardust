using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class RecalculateBadgeEvent : StardustJobInfo
	{
		public Guid JobResultId { get; set; }
		public DateOnlyPeriod Period { get; set; }
	}
}
