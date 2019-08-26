using System;
using System.Collections.Concurrent;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Workers
{
	public class WorkerWrapperService
	{
        private readonly Func<IWorkerWrapper> _workerWrapperFunc;
        private readonly NodeConfigurationService _nodeConfigurationService;
		private readonly ConcurrentDictionary<int, IWorkerWrapper> _workersByPort = new ConcurrentDictionary<int, IWorkerWrapper>();

		public WorkerWrapperService (Func<IWorkerWrapper> workerWrapperFunc, NodeConfigurationService nodeConfigurationService)
		{
            _workerWrapperFunc = workerWrapperFunc;
            _nodeConfigurationService = nodeConfigurationService;
		}

		public virtual IWorkerWrapper GetWorkerWrapperByPort(int port)
		{
			return _workersByPort.GetOrAdd(port, p =>
            {
                var w = _workerWrapperFunc.Invoke();
                w.Init(_nodeConfigurationService.GetConfigurationForPort(p));
                return w;
            });
		}
	}
}
