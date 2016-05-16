using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	[EnabledBy(Toggles.TestToggle)]
	public class HandlerUsedOnTestToggle : 
		IHandleEvent<TestToggleEvent>,
		IRunOnHangfire,
		IRecreatable,
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
	public class HandlerUsedOnTestToggle_WithMethodUsedOnTestToggle2 : 
		IHandleEvent<TestToggleEvent>,
		IHandleEvent<TestToggle2Event>,
		IRunOnHangfire,
		IRecreatable,
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
	public class HandlerNotUsedOnTestToggle :
		IHandleEvent<TestToggleEvent>,
		IRunOnHangfire,
		IRecreatable,
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

	
	public class HandlerMethodNotUsedOnTestToggle :
		IHandleEvent<TestToggleEvent>,
		IRunOnHangfire,
		IRecreatable,
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