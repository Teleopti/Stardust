using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	[UseOnToggle(Toggles.TestToggle)]
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

	[UseOnToggle(Toggles.TestToggle)]
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

		[UseOnToggle(Toggles.TestToggle2)]
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

	[UseNotOnToggle(Toggles.TestToggle)]
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
		[UseNotOnToggle(Toggles.TestToggle)]
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