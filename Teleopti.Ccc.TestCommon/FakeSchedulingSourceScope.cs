using System;
using System.Threading;
using Teleopti.Ccc.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeSchedulingSourceScope : ICurrentSchedulingSource, ISchedulingSourceScope
	{
		private readonly ThreadLocal<string> _threadSchedulingScope = new ThreadLocal<string>();
		private readonly ThreadLocal<string> _threadSchedulingScopeUsedToBe = new ThreadLocal<string>();

		public string UsedToBe()
		{
			return _threadSchedulingScopeUsedToBe.Value;
		}

		public string Current()
		{
			return _threadSchedulingScope.Value;
		}

		public IDisposable OnThisThreadUse(string schedulingScope)
		{
			_threadSchedulingScope.Value = schedulingScope;
			_threadSchedulingScopeUsedToBe.Value = schedulingScope;
			return new GenericDisposable(() => { _threadSchedulingScope.Value = null; });
		}
	}
}