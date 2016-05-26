using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public abstract class MultiplicatorDefinitionSetChangedBase : EventWithInfrastructureContext
	{
		public MultiplicatorType MultiplicatorType { get; set; }
		public Guid MultiplicatorDefinitionSetId { get; set; }
	}

	public class MultiplicatorDefinitionSetCreated : MultiplicatorDefinitionSetChangedBase
	{
	}

	public class MultiplicatorDefinitionSetChanged : MultiplicatorDefinitionSetChangedBase
	{
	}

	public class MultiplicatorDefinitionSetDeleted : MultiplicatorDefinitionSetChangedBase
	{
	}
}