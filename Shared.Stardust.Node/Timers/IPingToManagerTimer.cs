using System;

namespace Stardust.Node.Timers
{
	public interface IPingToManagerTimer: IDisposable
	{
		void SetupAndStart(NodeConfiguration nodeConfiguration);
	}
}
