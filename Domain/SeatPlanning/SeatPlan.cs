using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	[Serializable]
	public class SeatPlan : VersionedAggregateRootWithBusinessUnit, ISeatPlan
	{
		public virtual DateOnly Date { get; set; }
		public virtual SeatPlanStatus Status { get; set; }
	}
}