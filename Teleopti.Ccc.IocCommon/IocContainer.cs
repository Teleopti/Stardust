using Autofac;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon.Configuration;


namespace Teleopti.Ccc.IocCommon
{
    public static class IocContainer
    {
        public static IIocContainer Initialize(ContainerBuilder builder)
        {
            using(PerformanceOutput.ForOperation("Configure Ioc"))
                return new IocContainerImpl(builder.Build());
        }
    }
}