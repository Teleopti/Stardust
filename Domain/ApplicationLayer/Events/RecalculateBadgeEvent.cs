using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class RecalculateBadgeEvent : StardustJobInfo
	{
		public Guid JobResultId { get; set; }
		public DateOnlyPeriod Period { get; set; }
	}
}
