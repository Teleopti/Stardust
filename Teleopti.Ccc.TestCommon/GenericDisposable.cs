using System;

namespace Teleopti.Ccc.TestCommon
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public class GenericDisposable : IDisposable
	{
		private Action _disposeAction;

		public GenericDisposable(Action disposeAction)
		{
			_disposeAction = disposeAction;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
		public void Dispose()
		{
			_disposeAction.Invoke();
			_disposeAction = null;
		}
	}
}