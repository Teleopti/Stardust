using Autofac;
using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.IocCommon.Configuration
{
    public class EncryptionModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<OneWayEncryption>()
                .As<IOneWayEncryption>()
                .SingleInstance();
        }
    }
}
