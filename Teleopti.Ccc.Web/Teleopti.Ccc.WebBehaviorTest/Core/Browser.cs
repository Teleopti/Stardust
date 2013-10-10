using System;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.WatiNIE;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using WatiN.Core;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public static class Browser
	{
		private static readonly IDictionary<string, IBrowserActivator> Activators =
			new Dictionary<string, IBrowserActivator>
				{
					{"WatiN", new WatiNSingleBrowserIEActivator()}
				};

		private static IBrowserActivator _activator;

		static Browser()
		{
			_activator = Activators.First().Value;
		}

		public static IE Current
		{
			get
			{
				var activator = _activator as WatiNSingleBrowserIEActivator;
				return activator != null ? activator.Internal : null;
			}
		}

		public static IBrowserInteractions Interactions { get { return _activator.GetInteractions(); } }

		public static void Start(TimeSpan timeout, TimeSpan retry)
		{
			Activators.ForEach(a => a.Value.Start(timeout, retry));
			Timeouts.Timeout = timeout;
			Timeouts.Poll = retry;
		}

		public static void SelectBrowserByTag()
		{
			var activatorsWithMatchingTag = (
				                        from a in Activators
				                        let key = a.Key
				                        where ScenarioContext.Current.IsTaggedWith(key)
				                        select a.Value
			                        ).ToArray();
			_activator = activatorsWithMatchingTag.Any() ? 
				activatorsWithMatchingTag.First() : 
				Activators.First().Value;
		}

		public static IDisposable TimeoutScope(TimeSpan timeout)
		{
			return new TimeoutScope(_activator, timeout);
		}

		public static bool IsStarted()
		{
			return _activator.IsRunning();
		}

		public static void NotifyBeforeTestRun()
		{
			_activator.NotifyBeforeTestRun();
		}

		public static void NotifyBeforeScenario()
		{
			_activator.NotifyBeforeScenario();
		}

		public static void Close()
		{
			Activators.ForEach(a => a.Value.Close());
		}

	}
}
