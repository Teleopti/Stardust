using System;

namespace Teleopti.Ccc.TestCommon.IoC
{
	[AttributeUsage(AttributeTargets.Class)]
	public class UseEventPublisherAttribute : Attribute
	{
		public UseEventPublisherAttribute(Type eventPublisher)
		{
			EventPublisher = eventPublisher;
		}

		public Type EventPublisher { get; private set; }
	}
}