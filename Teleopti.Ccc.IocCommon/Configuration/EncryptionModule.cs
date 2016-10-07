using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Security;

namespace Teleopti.Ccc.IocCommon.Configuration
{
    public class EncryptionModule : Module
    {
	    private readonly IIocConfiguration _configuration;

	    public EncryptionModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
        {
	       
			if (_configuration.Toggle(Toggles.NewPasswordHash_40460))
			{
				registerAsDefault<BCryptHashFunction>(builder);
				registerAsExisting<OneWayEncryption>(builder);
			}
			else
			{
				registerAsDefault<OneWayEncryption>(builder);
				registerAsExisting<BCryptHashFunction>(builder);
			}
		}

	    private static void registerAsDefault<T>(ContainerBuilder builder)
	    {
			builder.RegisterType<T>()
				.As<IHashFunction>()
				.SingleInstance();
		}

	    private static void registerAsExisting<T>(ContainerBuilder builder)
	    {
		    builder.RegisterType<T>()
			    .As<IHashFunction>()
			    .SingleInstance()
			    .PreserveExistingDefaults();
	    }
    }
}
