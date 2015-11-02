using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.CoypuImpl;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver
{
	public static class BrowserInteractionsAngularExtensions
	{
		public static void SetScopeValues(this IBrowserInteractions interactions, string selector, Dictionary<string, string> values)
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
			var ts = new CancellationTokenSource(Timeouts.Timeout);
			var token = ts.Token;

			var condition = "scope." + name + " == " + value;
			var script = scopeByQuerySelector(selector) +						 
						 " return " + condition + "; ";

			var readerName = getTmpName(name);
			interactions.Javascript(waitForAngular(selector, script, readerName));

			Func<bool> checker = () =>
			{
				Boolean parsed;
				string checkReturn = scopeByQuerySelector(selector) + string.Format(" return scope.$result{0}; ", readerName);

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
					await Task.Delay(Timeouts.Poll, token);
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

		public static void AssertScopeValue(this IBrowserInteractions interactions, string selector, string name, Constraint constraint)
		{
			var query = scopeByQuerySelector(selector) + string.Format(" return scope.{0}; ", name);
			EventualAssert.That(() =>
			{
				var result = interactions.Javascript(query);				
				return result;
			}, constraint, () => "Failed to assert scope value " + name, new SeleniumExceptionCatcher());
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
		
		private static string waitForAngular(string selector, string next, string readerName)
		{
			return scopeByQuerySelector(selector)
				   + string.Format(" scope.$result{0} = null; ", readerName)
				   + string.Format("var bser = {0}.injector().get('$browser'); ", elementByQuerySelector(selector))
				   + string.Format("var cb = function() {{ scope.$result{0} = (function(){{ {1} }})(); }}; ", readerName, next)
				   + "bser.notifyWhenNoOutstandingRequests(cb); ";
		}

		private static string getTmpName(string input)
		{			
			var output = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(input));
			Regex rgx = new Regex("[^a-zA-Z0-9 -]");
			return rgx.Replace(output, "");
		}


	}
}
