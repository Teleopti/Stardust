using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	[Serializable]
	public class SeatPlan : AggregateRoot_Events_ChangeInfo_BusinessUnit, ISeatPlan
	{
		public virtual DateOnly Date { get; set; }
		public virtual SeatPlanStatus Status { get; set; }
	}
}