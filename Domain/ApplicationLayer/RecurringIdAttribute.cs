using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	// this Id is used to reference a recurring job in storage
	// and should not be changed
	[AttributeUsage(AttributeTargets.Method)]
	public class RecurringIdAttribute : Attribute
	{
		public RecurringIdAttribute(string id)
		{
			Id = id;
		}

		public string Id { get; }

	}
}