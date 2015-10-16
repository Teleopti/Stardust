using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere
{
	public class FakeClientBuilder
	{
		public dynamic Make(string methodName, Action action)
		{
			IDictionary<string, object> caller = new ExpandoObject();
			caller[methodName] = action;
			return caller;
		}

		public dynamic Make<T>(string methodName, Action<T> action)
		{
			IDictionary<string, object> caller = new ExpandoObject();
			caller[methodName] = action;
			return caller;
		}

		public dynamic Make<T, T2>(string methodName, Action<T, T2> action)
		{
			IDictionary<string, object> caller = new ExpandoObject();
			caller[methodName] = action;
			return caller;
		}
	}
}