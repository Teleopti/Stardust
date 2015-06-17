using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	[Serializable]
	public class SeatPlan : VersionedAggregateRootWithBusinessUnit, ISeatPlan
	{
		public DateOnly Date { get; set; }
		public SeatPlanStatus Status { get; set; }
	}
}