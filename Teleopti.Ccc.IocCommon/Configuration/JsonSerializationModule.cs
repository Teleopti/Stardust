using Autofac;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class JsonSerializationModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<EventSerializerSettings>();
			builder.RegisterType<NewtonsoftJsonSerializer>()
				.As<IJsonSerializer>()
				.As<IJsonDeserializer>()
				.As<IJsonEventSerializer>()
				.As<IJsonEventDeserializer>()
				.SingleInstance();
		}
	}
}