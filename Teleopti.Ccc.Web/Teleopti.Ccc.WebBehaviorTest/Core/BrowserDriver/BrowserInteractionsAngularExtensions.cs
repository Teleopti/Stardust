using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver
{
	public static class BrowserInteractionsAngularExtensions
	{
		public static void FillScopeValues(this IBrowserInteractions interactions, string selector, Dictionary<string, string> values)
		{			
			var assignments = values.Select(kvp => "scope." + kvp.Key + " = " + kvp.Value + "; ").Aggregate((acc, v) => acc + v);
			var script = scopeByQuerySelector(selector) +
						 runnerByQuerySelector(selector) +
						 "runner(function() {" + assignments + " });";

			interactions.Javascript(script);		
		}

		public static void WaitScopeCondition(this IBrowserInteractions interactions, string selector, string name,
			string value, Action actionThen)
		{
			var ts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
			var token = ts.Token;

			var condition = "scope." + name + " == " + value;
			var script = scopeByQuerySelector(selector) +						 
						 " return " + condition + "; ";

			interactions.Javascript(waitForAngular(selector, script));

			Func<bool> checker = () =>
			{
				Boolean parsed;
				string checkReturn = scopeByQuerySelector(selector) + " return scope.$runJavascriptResult; ";

				var result = interactions.Javascript(checkReturn);
				if (Boolean.TryParse(result, out parsed))
				{
					return parsed;
				}
				return false;
			};

			var task = Task.Factory.StartNew(async () =>
			{
				while (!checker())
				{
					await Task.Delay(TimeSpan.FromSeconds(1), token);
					token.ThrowIfCancellationRequested();
				}
			}, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);

			task.Wait(token);

			if (task.IsFaulted || task.IsCanceled)
			{
				Assert.Fail("Failed to meet condition: " + name + " - " + value);
			}
			
			actionThen();						
		}

		private static string elementByQuerySelector(string selector)
		{
			return string.Format("angular.element(document.querySelector('{0}'))", selector);
		}

		private static string scopeByQuerySelector(string selector)
		{
			return string.Format("var scope = {0}.scope(); ", elementByQuerySelector(selector));
		}

		private static string runnerByQuerySelector(string selector)
		{
			return string.Format("var runner = {0}.injector().get('$timeout'); ", elementByQuerySelector(selector));
		}
		
		private static string waitForAngular(string selector, string next)
		{
			return scopeByQuerySelector(selector)
				   + " scope.$runJavascriptResult = null; "
				   + string.Format("var bser = {0}.injector().get('$browser'); ", elementByQuerySelector(selector))
				   + string.Format("var cb = function() {{ scope.$runJavascriptResult = (function(){{ {0} }})(); }}; ", next)
				   + "bser.notifyWhenNoOutstandingRequests(cb); ";
		}


	}
}
