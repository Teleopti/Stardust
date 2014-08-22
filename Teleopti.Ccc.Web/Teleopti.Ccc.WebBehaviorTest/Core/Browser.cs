using System;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.CoypuImpl;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public static class Browser
	{
		private class RegisteredActivator
		{
			public string Tag { get; set; }
			public IBrowserActivator Activator { get; set; }
			public bool Visible { get; set; }
			public bool Started { get; set; }
		}

		private static readonly IList<RegisteredActivator> Activators =
			new List<RegisteredActivator>
				{
					new RegisteredActivator
						{
							Tag = "Chrome",
							Activator = new CoypuChromeActivator(),
							Visible = true
						}
				};

		private static RegisteredActivator _activator;

		static Browser()
		{
			_activator = Activators.First();
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

		public static void SelectDefaultVisibleBrowser()
		{
			_activator = Activators.First(x => x.Visible);
		}

		public static void SelectBrowserByTag()
		{
			var activatorsWithMatchingTag = Activators
				.Where(a => ScenarioContext.Current.IsTaggedWith(a.Tag))
				.ToArray();
			_activator = activatorsWithMatchingTag.Any() ? 
				activatorsWithMatchingTag.First() : 
				Activators.First();
		}

		public static IDisposable TimeoutScope(TimeSpan timeout)
		{
			return new TimeoutScope(GetStartedActivator(), timeout);
		}

		public static void Close()
		{
			Activators.ForEach(a => a.Activator.Close());
		}
	}
}
