using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
    public class ApproveRequestsWithValidatorsEventStardustHandler : IHandle<ApproveRequestsWithValidatorsEvent>,
        IRunOnStardust
    {
        private readonly IComponentContext _componentContext;

        public ApproveRequestsWithValidatorsEventStardustHandler(IComponentContext componentContext)
        {
            _componentContext = componentContext;
        }

        [AsSystem]
        public virtual void Handle(ApproveRequestsWithValidatorsEvent @event,
            CancellationTokenSource cancellationTokenSource,
            Action<string> sendProgress)
        {
            var theRealOne = _componentContext.Resolve<IHandleEvent<ApproveRequestsWithValidatorsEvent>>();
            theRealOne.Handle(@event);
        }
    }
} 
