using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
			Constraint constraint, Action actionThen)
		{
			AssertScopeValue(interactions, selector, name, constraint);			
			actionThen();						
		}

		public static void AssertScopeValue(this IBrowserInteractions interactions, string selector, string name, Constraint constraint)
		{
			var script = string.Format(scopeByQuerySelector(selector) + " return scope.{0}; ", name);

			var readerName = getTmpName(name);

			interactions.Javascript(waitForAngular(selector, script, readerName));

			EventualAssert.That(() =>
			{
				var query = scopeByQuerySelector(selector) + string.Format(" return scope.$result{0}; ", readerName);
				var result = interactions.Javascript(query);
				if (result != null) result = result.ToLower();
				
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
			var output = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
			Regex rgx = new Regex("[^a-zA-Z0-9 -]");
			return rgx.Replace(output, "");
		}


	}
}
