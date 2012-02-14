using TechTalk.SpecFlow;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Extensions
{
	public static class ScenarioContextExtensions
	{

		public static T Value<T>(this ScenarioContext context)
		{
			return Value<T>(context, typeof (T).FullName);
		}

		public static T Value<T>(this ScenarioContext context, T value) where T : class
		{
			return Value(context, typeof (T).FullName, value);
		}

		public static T Value<T>(this ScenarioContext context, string key)
		{
			if (context.ContainsKey(key))
				return (T)context[key];
			return default(T);
		}

		public static T Value<T>(this ScenarioContext context, string key, T value) where T :class
		{
			if (value == null)
			{
				if (ScenarioContext.Current.ContainsKey(key))
					ScenarioContext.Current.Remove(key);
			}
			else
				ScenarioContext.Current[key] = value;
			return value;
		}
	}
}