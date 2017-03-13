﻿using System;
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
			public Exception StartException { get; set; }
		}

		private static IBrowserActivator _noBrowser = new NoBrowserActivator();

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
					Activator = _noBrowser
				}
			};

		private static registeredActivator _activator;
		private static TimeSpan _timeout;
		private static TimeSpan _retry;

		static Browser()
		{
			_activator = activators.First();
		}

		public static bool IsStarted => _activator.Started;

		private static IBrowserActivator getActivator()
		{
			if (_activator.StartException != null)
				throw _activator.StartException;
			if (!_activator.Started)
			{
				try
				{
					_activator.Activator.Start(_timeout, _retry);
				}
				catch (Exception e)
				{
					_activator.StartException = e;
					throw;
				}
				_activator.Started = true;
			}
			return _activator.Activator;
		}

		public static IBrowserInteractions Interactions => getActivator().GetInteractions();

		public static void SetDefaultTimeouts(TimeSpan timeout, TimeSpan retry)
		{
			_timeout = timeout;
			_retry = retry; 
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
			return new TimeoutScope(getActivator(), timeout);
		}

		public static void Dispose()
		{
			activators.ForEach(a => a.Activator.Dispose());
		}
	}
}
