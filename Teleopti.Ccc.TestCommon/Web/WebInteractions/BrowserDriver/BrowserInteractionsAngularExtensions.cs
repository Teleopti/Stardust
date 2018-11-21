using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver
{
	public static class BrowserInteractionsAngularExtensions
	{
		public static void SetScopeValues(this IBrowserInteractions interactions, string selector, Dictionary<string, string> values, bool useIsolateScope = false, string actionName = null)
		{			
			var assignments = values.Select(kvp => "scope." + kvp.Key + " = " + kvp.Value + "; ").Aggregate((acc, v) => acc + v);
			var runAction = actionName == null ? "" : " scope." + actionName + "(); ";

			var script = scopeByQuerySelector(selector, useIsolateScope) +
						 runnerByQuerySelector(selector) +
						 "runner(function() {" + assignments +  runAction + " });";		
		
			interactions.Javascript_IsFlaky(script);		
		}

		public static void InvokeServiceAction(this IBrowserInteractions interactions, string selector, string serviceName, string actionName)
		{
			var script = getInjectableService(selector, serviceName) + string.Format(" service.{0}();", actionName);
			interactions.Javascript_IsFlaky(script);
		}

		public static void InvokeScopeAction(this IBrowserInteractions interactions, string selector, string actionName,
			bool useIsolateScope = false)
		{
			var run = $" scope.{actionName}();";
			var script = scopeByQuerySelector(selector, useIsolateScope) +
						 runnerByQuerySelector(selector) +
						 "runner(function() {" + run + " });";
			interactions.Javascript_IsFlaky(script);
		}

		public static void WaitScopeCondition<T>(this IBrowserInteractions interactions, string selector, string name,
			T constraint, Action actionThen, bool useIsolateScope = false)
		{
			AssertScopeValue(interactions, selector, name, constraint, useIsolateScope);			
			actionThen();						
		}
	
		public static void AssertScopeValue<T>(this IBrowserInteractions interactions, string selector, string name, T constraint, bool useIsolateScope = false)
		{		
			var script = string.Format(scopeByQuerySelector(selector, useIsolateScope) + " return scope.{0}; ", name);
			var readerName = getTmpName(name);
		
			interactions.Javascript_IsFlaky(waitForAngular(selector, script, readerName, useIsolateScope));
			
			if (typeof (T) == typeof (bool))
			{
				var query = scopeByQuerySelector(selector, useIsolateScope) +
							($" if (scope.$result{readerName} == 'I_am_a_dummy_value') return 'Unassigned'; " +
							 $" return scope.$result{readerName} ? 'True': 'False' ; ");
				interactions.AssertJavascriptResultContains(query, constraint.ToString());				
			}		
			else 
			{
				var query = scopeByQuerySelector(selector, useIsolateScope) +
							$" return scope.$result{readerName}; ";
				
				interactions.AssertJavascriptResultContains(query, constraint.ToString());				
			}		
		}

		private static string elementByQuerySelector(string selector)
		{
			return string.Format("angular.element(document.querySelector('{0}'))", selector);
		}

		private static string scopeByQuerySelector(string selector, bool useIsolateScope)
		{
			return string.Format("var scope = {0}.{1}(); ", elementByQuerySelector(selector), useIsolateScope?"isolateScope":"scope");
		}

		private static string runnerByQuerySelector(string selector)
		{
			return string.Format("var runner = {0}.injector().get('$timeout'); ", elementByQuerySelector(selector));
		}

		private static string getInjectableService(string selector, string serviceName)
		{
			return string.Format("var service = {0}.injector().get('{1}'); ", elementByQuerySelector(selector), serviceName);
		}

		private static string waitForAngular(string selector, string next, string readerName, bool useIsolateScope)
		{
			return scopeByQuerySelector(selector, useIsolateScope)
				   + runnerByQuerySelector(selector)				
				   + string.Format("var evaluator = function(){{ {0} }}; ", next)
				   + string.Format("scope.$result{0} = 'I_am_a_dummy_value'; ", readerName)
				   + string.Format("var setv = function(v) {{ scope.$result{0} = v; }}; ", readerName)
				   + string.Format("runner(function() {{ scope.$result{0} = null; scope.$watch(evaluator, function(v) {{ setv(v); }}); }}, 200); ", readerName);			
		}

		private static string getTmpName(string input)
		{
			var output = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
			Regex rgx = new Regex("[^a-zA-Z0-9 -]");
			return rgx.Replace(output, "");
		}
	}
}
