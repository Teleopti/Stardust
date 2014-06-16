using Autofac;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class ResourceHandlerModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			registerCommonTypes(builder);
		}

		private static void registerCommonTypes(ContainerBuilder builder)
		{
			builder.RegisterType<UserTextTranslator>().As<IUserTextTranslator>().SingleInstance();
		}
	}
}