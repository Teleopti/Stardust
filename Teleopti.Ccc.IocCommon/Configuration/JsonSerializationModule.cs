using Autofac;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class JsonSerializationModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<NewtonsoftJsonSerializer>().As<IJsonSerializer>().SingleInstance();
			builder.RegisterType<NewtonsoftJsonDeserializer>().As<IJsonDeserializer>().SingleInstance();
		}
	}
}