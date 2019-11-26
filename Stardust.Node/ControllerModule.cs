using Autofac;
using Autofac.Integration.WebApi;

namespace Stardust.Node
{
    public class ControllerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterApiControllers(typeof (NodeController).Assembly);
        }
    }
}
