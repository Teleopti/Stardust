using System;

namespace Teleopti.Ccc.DBManager.Library
{
	public class GenericDisposable : IDisposable
	{
		private Action _disposeAction;

		public GenericDisposable(Action disposeAction)
		{
			_disposeAction = disposeAction;
		}

		public void Dispose()
		{
			_disposeAction.Invoke();
			_disposeAction = null;
		}
	}
}