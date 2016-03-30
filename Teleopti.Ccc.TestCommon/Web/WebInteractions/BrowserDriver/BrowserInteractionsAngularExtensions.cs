using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver
{
	public static class BrowserInteractionsAngularExtensions
	{
		public static void SetScopeValues(this IBrowserInteractions interactions, string selector, Dictionary<string, string> values, bool useIsolateScope = false)
		{			
			var assignments = values.Select(kvp => "scope." + kvp.Key + " = " + kvp.Value + "; ").Aggregate((acc, v) => acc + v);
			var script = scopeByQuerySelector(selector, useIsolateScope) +
						 runnerByQuerySelector(selector) +
						 "runner(function() {" + assignments + " });";

			interactions.Javascript(script);		
		}

		public static void WaitScopeCondition<T>(this IBrowserInteractions interactions, string selector, string name,
			T constraint, Action actionThen, bool useIsolateScope = false)
		{
			AssertScopeValue(interactions, selector, name, constraint, useIsolateScope);			
			actionThen();						
		}

		public static void AssertScopeValueEmpty(this IBrowserInteractions interactions, string selector, string name, bool useIsolateScope = false)
		{
			var script = string.Format(scopeByQuerySelector(selector, useIsolateScope) + " return scope.{0}; ", name);
			var readerName = getTmpName(name);
			interactions.Javascript(waitForAngular(selector, script, readerName, useIsolateScope));
			var query = scopeByQuerySelector(selector, useIsolateScope) +
						string.Format("return scope.$result{0} == null ?'True': 'False' ; ", readerName);
			
			interactions.AssertJavascriptResultContains(query, "True");		
		}

		public static void AssertScopeValue<T>(this IBrowserInteractions interactions, string selector, string name, T constraint, bool useIsolateScope = false)
		{		
			var script = string.Format(scopeByQuerySelector(selector, useIsolateScope) + " return scope.{0}; ", name);
			var readerName = getTmpName(name);
		
			interactions.Javascript(waitForAngular(selector, script, readerName, useIsolateScope));
			
			if (typeof (T) == typeof (bool))
			{
				var query = scopeByQuerySelector(selector, useIsolateScope) +
							string.Format(" if (scope.$result{0} == 'I_am_a_dummy_value') return 'Unassigned'; " +
										  " return scope.$result{1} ? 'True': 'False' ; ", readerName, readerName);
				interactions.AssertJavascriptResultContains(query, constraint.ToString());				
			}		
			else 
			{
				var query = scopeByQuerySelector(selector, useIsolateScope) +
							string.Format(" return scope.$result{0}; ", readerName);
				
				interactions.AssertJavascriptResultContains(query, constraint.ToString());				
			}		
		}

		public static void AngularSelectOptionByText(this IBrowserInteractions interactions, string selector, string text, bool useIsolateScope = false)
		{
			var selectSelector = selector + ":enabled";
			var optionSelector = string.Format(selectSelector + " option:contains('{0}')", text);
			interactions.AssertExists(selectSelector);
			interactions.AssertExistsUsingJQuery(optionSelector);
			var selectAction = string.Format("$(\"{0}\").val($(\"{1}\").val());" ,
									   selectSelector.JSEncode(), optionSelector.JSEncode());
			var triggerChange = string.Format("{0}.triggerHandler(\"change\")", elementByQuerySelector(selector));

			var script = runScript(selector, selectAction + triggerChange, useIsolateScope);
			interactions.Javascript(script);
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

		private static string runScript(string selector, string script, bool useIsolateScope)
		{
			return scopeByQuerySelector(selector, useIsolateScope)
				   + runnerByQuerySelector(selector)
				   + string.Format("runner(function() {{ {0} }}, 200); ", script);			

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
