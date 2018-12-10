using System;
using System.Threading;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;


namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class CurrentSchedulingSource : ICurrentSchedulingSource, ISchedulingSourceScope
	{
		private readonly ThreadLocal<string> _threadSchedulingScope = new ThreadLocal<string>();

		public string Current()
		{
			return _threadSchedulingScope.Value;
		}

		public IDisposable OnThisThreadUse(string schedulingScope)
		{
			_threadSchedulingScope.Value = schedulingScope;
			return new GenericDisposable(() => { _threadSchedulingScope.Value = null; });
		}
	}
}