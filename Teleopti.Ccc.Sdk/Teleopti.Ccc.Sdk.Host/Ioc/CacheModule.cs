using Autofac;
using Teleopti.Ccc.Infrastructure.Foundation.Cache;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfHost.Ioc
{
    public class CacheModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof (AspNetCache<>))
                .As(typeof (ICustomDataCache<>))
                .SingleInstance();
        }
    }
}