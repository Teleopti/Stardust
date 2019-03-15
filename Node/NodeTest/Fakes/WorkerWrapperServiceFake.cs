using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Stardust.Node;
using Stardust.Node.Interfaces;
using Stardust.Node.Workers;

namespace NodeTest.Fakes
{
    public class WorkerWrapperServiceFake : WorkerWrapperService
    {

        public IWorkerWrapper WorkerWrapper;

        public WorkerWrapperServiceFake(ILifetimeScope componentContext = null,
            NodeConfigurationService nodeConfigurationService = null) : base(null,null )
        {

        }


        public override IWorkerWrapper GetWorkerWrapperByPort(int port)
        {
            return WorkerWrapper;
        }



    }
}
