using Autofac;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class JsonSerializationModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<NewtonsoftJsonSerializer>()
				.As<IJsonSerializer>()
				.As<IJsonDeserializer>()
				.As<IJsonEventSerializer>()
				.As<IJsonEventDeserializer>()
				.SingleInstance();
		}
	}
}