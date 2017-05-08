using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	// yep, that didnt work (you'll know when u try)
	// count: 1

	[EnabledBy(Toggles.TestToggle)]
	public class HandlerEnabledByTestToggle : 
		IHandleEvent<TestToggleEvent>,
		IRunOnHangfire
	{
		public void Handle(TestToggleEvent @event)
		{
		}

		public void DeleteAll()
		{
		}
	}

	public class TestToggleEvent : IEvent
	{
	}

	[EnabledBy(Toggles.TestToggle)]
	public class HandlerEnabledByTestToggle_WithMethodEnabledByTestToggle2 : 
		IHandleEvent<TestToggleEvent>,
		IHandleEvent<TestToggle2Event>,
		IRunOnHangfire
	{
		public void Handle(TestToggleEvent @event)
		{
		}

		[EnabledBy(Toggles.TestToggle2)]
		public void Handle(TestToggle2Event @event)
		{
		}

		public void DeleteAll()
		{
		}
	}

	public class TestToggle2Event : IEvent
	{
	}

	[DisabledBy(Toggles.TestToggle)]
	public class HandlerDisabledByTestToggle :
		IHandleEvent<TestToggleEvent>,
		IRunOnHangfire
	{
		public void Handle(TestToggleEvent @event)
		{
		}

		public void DeleteAll()
		{
		}
	}

	
	public class HandlerMethodDisabledByTestToggle :
		IHandleEvent<TestToggleEvent>,
		IRunOnHangfire
	{
		[DisabledBy(Toggles.TestToggle)]
		public void Handle(TestToggleEvent @event)
		{
		}
	}



	[EnabledBy(Toggles.TestToggle)]
	public class PackageHandlerEnabledByTestToggle :
		IHandleEvents,
		IRunOnHangfire
	{
		public void Subscribe(SubscriptionRegistrator registrator)
		{
		}

		public void Handle(IEnumerable<IEvent> events)
		{
		}
	}
	

}