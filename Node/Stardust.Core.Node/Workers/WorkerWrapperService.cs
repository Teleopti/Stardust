using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stardust.Core.Node.Interfaces;
//using Autofac;

namespace Stardust.Core.Node.Workers
{
	public class WorkerWrapperService
	{
		private readonly NodeConfigurationService _nodeConfigurationService;
		private readonly Dictionary<int, IWorkerWrapper> _workersByPort = new Dictionary<int, IWorkerWrapper>();

		public WorkerWrapperService (IServiceProvider serviceProvider, NodeConfigurationService nodeConfigurationService)
		{
			_nodeConfigurationService = nodeConfigurationService;
            _serviceProvider = serviceProvider;
		}

		private IServiceProvider _serviceProvider{ get; }

		public virtual IWorkerWrapper GetWorkerWrapperByPort(int port)
		{
			IWorkerWrapper worker;
			if (_workersByPort.TryGetValue(port, out worker)) return worker;

			worker = _serviceProvider.GetService<IWorkerWrapper>();
			worker.Init(_nodeConfigurationService.GetConfigurationForPort(port));
			_workersByPort.Add(port, worker);
			return worker;
		}
	}
}
