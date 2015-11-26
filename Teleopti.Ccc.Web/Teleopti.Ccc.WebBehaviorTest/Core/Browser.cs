using System;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver.CoypuImpl;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public static class Browser
	{
		private class registeredActivator
		{
			public string Tag { get; set; }
			public IBrowserActivator Activator { get; set; }
			public bool Started { get; set; }
		}

		private static readonly IEnumerable<registeredActivator> activators =
			new List<registeredActivator>
				{
					new registeredActivator
						{
							Tag = "Chrome",
							Activator = new CoypuChromeActivator()
						},
					new registeredActivator
					{
						Tag = "NoBrowser",
						Activator =  new NoBrowserActivator()
					}
				};

		private static registeredActivator _activator;

		static Browser()
		{
			_activator = activators.First();
		}

		private static IBrowserActivator GetStartedActivator()
		{
			if (!_activator.Started)
			{
				_activator.Activator.Start(Timeouts.Timeout, Timeouts.Poll);
				_activator.Started = true;
			}
			return _activator.Activator;
		}

		public static IBrowserInteractions Interactions { get { return GetStartedActivator().GetInteractions(); } }

		public static void SetDefaultTimeouts(TimeSpan timeout, TimeSpan retry)
		{
			Timeouts.Timeout = timeout;
			Timeouts.Poll = retry;
		}

		public static void SelectBrowserByTag()
		{
			var activatorsWithMatchingTag = activators
				.Where(a => ScenarioContext.Current.IsTaggedWith(a.Tag))
				.ToArray();
			_activator = activatorsWithMatchingTag.Any() ? 
				activatorsWithMatchingTag.First() : 
				activators.First();
		}

		public static IDisposable TimeoutScope(TimeSpan timeout)
		{
			return new TimeoutScope(GetStartedActivator(), timeout);
		}

		public static void Close()
		{
			activators.ForEach(a => a.Activator.Close());
		}
	}
}
