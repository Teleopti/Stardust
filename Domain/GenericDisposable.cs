using System;

namespace Teleopti.Ccc.Domain
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
			if (_disposeAction != null)
			{
				_disposeAction.Invoke();
				_disposeAction = null;
			}
		}
	}
}