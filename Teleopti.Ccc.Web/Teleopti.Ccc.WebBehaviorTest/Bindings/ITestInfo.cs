using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	public interface ITestInfo
	{
		string Name();
		Exception Error();
		bool IsTaggedWith(string tag);
		IEnumerable<string> Tags();
	}

	public class NUnitTest : ITestInfo
	{
		private readonly ITest _test;

		public NUnitTest(ITest test)
		{
			_test = test;
		}

		public string Name() => _test.Name;
		public Exception Error() => null;
		public bool IsTaggedWith(string tag) => false;
		public IEnumerable<string> Tags() => Enumerable.Empty<string>();
	}

	public class ScenarioTest : ITestInfo
	{
		public string Name() => ScenarioContext.Current.ScenarioInfo.Title;
		public Exception Error() => ScenarioContext.Current.TestError;
		public bool IsTaggedWith(string tag) => ScenarioContext.Current.IsTaggedWith(tag);
		public IEnumerable<string> Tags() => ScenarioContext.Current.AllTags();
	}
}