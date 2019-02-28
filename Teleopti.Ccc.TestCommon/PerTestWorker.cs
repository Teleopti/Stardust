using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Teleopti.Ccc.TestCommon
{
	public class PerTestWorker<T>
	{
		private readonly Dictionary<string, T> _values = new Dictionary<string, T>();

		public void Set(Func<T> value)
		{
			lock (_values)
			{
				_values[TestContext.CurrentContext.WorkerId] = value.Invoke();
			}
		}

		public int WorkerCount() => _values.Count;

		public T Value
		{
			get
			{
				if (_values.ContainsKey(TestContext.CurrentContext.WorkerId))
					return _values[TestContext.CurrentContext.WorkerId];
				return default(T);
			}
		}
	}
}