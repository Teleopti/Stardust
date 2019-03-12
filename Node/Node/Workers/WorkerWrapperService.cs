using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Workers
{
	public class WorkerWrapperService
	{
		private readonly NodeConfigurationService _nodeConfigurationService;
		private Dictionary<int, IWorkerWrapper> _workersByPort = new Dictionary<int, IWorkerWrapper>();

		public WorkerWrapperService (ILifetimeScope componentContext, NodeConfigurationService nodeConfigurationService)
		{
			if (componentContext == null)
			{
				throw new ArgumentNullException(nameof(componentContext));
			}

			_nodeConfigurationService = nodeConfigurationService;

			ComponentContext = componentContext;
		}

		private ILifetimeScope ComponentContext { get; }

		public IWorkerWrapper GetWorkerWrapperByPort(int port)
		{
			//using (var lifetimeScope = ComponentContext.BeginLifetimeScope())
			{
				IWorkerWrapper worker;
				if (_workersByPort.TryGetValue(port, out worker)) return worker;

				worker = ComponentContext.Resolve<IWorkerWrapper>();
				worker.Init(_nodeConfigurationService.GetConfigurationForPort(port));
				_workersByPort.Add(port, worker);
				return worker;
			}
		}
	}
}
