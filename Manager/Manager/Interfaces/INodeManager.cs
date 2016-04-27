using System;

namespace Stardust.Manager.Interfaces
{
	public interface INodeManager
	{
		void AddWorkerNode(Uri workerNodeUri);

		void RequeueJobsThatDidNotFinishedByWorkerNodeUri(Uri workerNodeUri, bool keepJobDetailsIfExists);

		void WorkerNodeRegisterHeartbeat(string nodeUri);
	}
}