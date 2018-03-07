using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class RecalculateBadgeEvent : StardustJobInfo
	{
		public Guid JobResultId { get; set; }
	}
}
