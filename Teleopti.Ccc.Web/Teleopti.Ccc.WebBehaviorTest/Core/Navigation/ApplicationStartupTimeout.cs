using System;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Navigation
{
	public class ApplicationStartupTimeout : INavigationInterceptor
	{
		private IDisposable _timeoutScope;

		public void Before(GotoArgs args)
		{
			_timeoutScope = Browser.TimeoutScope(TimeSpan.FromSeconds(60));
		}

		public void After(GotoArgs args)
		{
			_timeoutScope.Dispose();
			_timeoutScope = null;
		}
	}
}