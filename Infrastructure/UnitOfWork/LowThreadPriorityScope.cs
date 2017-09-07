using System;
using System.Threading;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class LowThreadPriorityScope : ILowThreadPriorityScope
	{
		private readonly ThreadLocal<ThreadPriority> _threadOldPriority = new ThreadLocal<ThreadPriority>();
		private readonly IToggleManager _toggleManager;

		public LowThreadPriorityScope(IToggleManager toggleManager)
		{
			_toggleManager = toggleManager;
		}

		public IDisposable OnThisThread()
		{
			if (_toggleManager.IsEnabled(Toggles.WFM_WebScheduling_LowPriority_44320))
			{
				_threadOldPriority.Value = Thread.CurrentThread.Priority;
				Thread.CurrentThread.Priority = ThreadPriority.Lowest;
				return new GenericDisposable(() =>
				{
					Thread.CurrentThread.Priority = _threadOldPriority.Value;
				});
			}
			return new GenericDisposable(() => { });
		}
	}
}