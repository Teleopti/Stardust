using System;
using Autofac;
using Stardust.Node;
using Stardust.Node.Interfaces;
using Stardust.Node.Workers;

namespace NodeTest.Fakes
{
    public class WorkerWrapperServiceFake : WorkerWrapperService
    {
        public IWorkerWrapper WorkerWrapper;

        public WorkerWrapperServiceFake(Func<IWorkerWrapper> workerWrapperFunc = null,
            NodeConfigurationService nodeConfigurationService = null) : base(workerWrapperFunc,nodeConfigurationService)
        {

        }

        public override IWorkerWrapper GetWorkerWrapperByPort(int port)
        {
            return WorkerWrapper;
        }
    }
}
