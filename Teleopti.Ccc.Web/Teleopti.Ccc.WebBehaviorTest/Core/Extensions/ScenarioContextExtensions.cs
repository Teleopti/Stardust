using System.Linq;
using TechTalk.SpecFlow;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Extensions
{
	public static class ScenarioContextExtensions
	{

		public static T Value<T>(this ScenarioContext context)
		{
			return Value<T>(context, typeof (T).FullName);
		}

		public static T Value<T>(this ScenarioContext context, T value)
		{
			return Value(context, typeof (T).FullName, value);
		}

		public static T Value<T>(this ScenarioContext context, string key)
		{
			if (context.ContainsKey(key))
				return (T)context[key];
			return default(T);
		}

		public static T Value<T>(this ScenarioContext context, string key, T value)
		{
			if (value == null)
			{
				if (context.ContainsKey(key))
					context.Remove(key);
			}
			else
				context[key] = value;
			return value;
		}

		public static bool IsTaggedWith(this ScenarioContext context, string tag)
		{
			if (FeatureContext.Current.FeatureInfo.Tags.Any(s => s == tag))
				return true;
			if (context.ScenarioInfo.Tags.Any(s => s == tag))
				return true;
			return false;
		}
	}
}