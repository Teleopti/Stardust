using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	[EnabledBy(Toggles.TestToggle)]
	public class HandlerEnabledByTestToggle : 
		IHandleEvent<TestToggleEvent>,
		IRunOnHangfire,
		IInitializeble
	{
		public void Handle(TestToggleEvent @event)
		{
		}

		public void DeleteAll()
		{
		}

		public bool Initialized()
		{
			return true;
		}
	}

	public class TestToggleEvent : IEvent
	{
	}

	[EnabledBy(Toggles.TestToggle)]
	public class HandlerEnabledByTestToggle_WithMethodEnabledByTestToggle2 : 
		IHandleEvent<TestToggleEvent>,
		IHandleEvent<TestToggle2Event>,
		IRunOnHangfire,
		IInitializeble
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

		public bool Initialized()
		{
			return true;
		}
	}

	public class TestToggle2Event : IEvent
	{
	}

	[DisabledBy(Toggles.TestToggle)]
	public class HandlerDisabledByTestToggle :
		IHandleEvent<TestToggleEvent>,
		IRunOnHangfire,
		IInitializeble
	{
		public void Handle(TestToggleEvent @event)
		{
		}

		public void DeleteAll()
		{
		}

		public bool Initialized()
		{
			return true;
		}
	}

	
	public class HandlerMethodDisabledByTestToggle :
		IHandleEvent<TestToggleEvent>,
		IRunOnHangfire,
		IInitializeble
	{
		[DisabledBy(Toggles.TestToggle)]
		public void Handle(TestToggleEvent @event)
		{
		}

		public void DeleteAll()
		{
		}

		public bool Initialized()
		{
			return true;
		}
	}
}