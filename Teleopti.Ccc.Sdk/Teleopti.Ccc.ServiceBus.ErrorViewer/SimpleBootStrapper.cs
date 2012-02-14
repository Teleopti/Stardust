using Castle.Core;
using Rhino.ServiceBus.Hosting;
using Rhino.ServiceBus.Internal;
using Rhino.ServiceBus.Sagas.Persisters;
using Castle.MicroKernel.Registration;

namespace Teleopti.Ccc.ServiceBus.ErrorViewer
{
    public class SimpleBootStrapper: AbstractBootStrapper
    {
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            
            container.Register(
                Component.For(typeof(ISagaPersister<>))
                    .ImplementedBy(typeof(InMemorySagaPersister<>))
                );
            container.Register(
                Component.For(typeof(IIncomingMessageHandler)).ImplementedBy(typeof(IncomingMessageHandler)).LifeStyle.Is(
                    LifestyleType.Singleton));
        }
    }
}
