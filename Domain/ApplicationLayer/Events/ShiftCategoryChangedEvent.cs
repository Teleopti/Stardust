using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ShiftCategoryChangedEvent : EventWithInfrastructureContext
	{
		public Guid ShiftCategoryId { get; set; }
	}

	public class ShiftCategoryDeletedEvent : EventWithInfrastructureContext
	{
	}
}