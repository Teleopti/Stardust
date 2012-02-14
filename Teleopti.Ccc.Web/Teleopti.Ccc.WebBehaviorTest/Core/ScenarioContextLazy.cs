using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public class ScenarioContextLazy<T> where T : class
	{
		private readonly Func<T> _getter;

		public ScenarioContextLazy(Func<T> getter)
		{
			_getter = getter;
		}

		public T Value
		{
			get
			{
				if (ScenarioContext.Current.Value<T>() == null)
					ScenarioContext.Current.Value(_getter.Invoke());
				return ScenarioContext.Current.Value<T>();
			}
		}

	}
}