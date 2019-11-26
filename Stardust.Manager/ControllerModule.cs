using Autofac;
using Autofac.Integration.WebApi;

namespace Stardust.Manager
{
   public class ControllerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterApiControllers(typeof(ManagerController).Assembly);
        }
    }
}
