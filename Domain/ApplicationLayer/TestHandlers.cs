using System;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	[UseOnToggle(Toggles.TestToggle)]
	public class HandlerUsedOnTestToggle : 
		IHandleEvent<TestToggleEvent>
	{
		public void Handle(TestToggleEvent @event)
		{
		}
	}

	public class TestToggleEvent : IEvent
	{
	}

	[UseOnToggle(Toggles.TestToggle)]
	public class HandlerUsedOnTestToggle_WithMethodUsedOnTestToggle2 : 
		IHandleEvent<TestToggleEvent>,
		IHandleEvent<TestToggle2Event>
	{
		public void Handle(TestToggleEvent @event)
		{
		}

		[UseOnToggle(Toggles.TestToggle2)]
		public void Handle(TestToggle2Event @event)
		{
		}
	}

	public class TestToggle2Event : IEvent
	{
	}

}