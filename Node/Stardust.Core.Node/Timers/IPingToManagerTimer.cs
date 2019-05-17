using System;

namespace Stardust.Core.Node.Timers
{
	public interface IPingToManagerTimer: IDisposable
	{
		void SetupAndStart(NodeConfiguration nodeConfiguration);
	}
}
