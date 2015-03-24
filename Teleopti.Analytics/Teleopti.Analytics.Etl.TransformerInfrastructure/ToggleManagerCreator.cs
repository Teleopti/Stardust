using Autofac;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
	public static class ToggleManagerCreator
	{
		public static IToggleManager Create()
		{
			var builder = new ContainerBuilder();
			var configuration = new IocConfiguration(new IocArgs(), CommonModule.ToggleManagerForIoc());
			builder.RegisterModule(new CommonModule(configuration));
			using (var tempContainer = builder.Build())
			{
				return tempContainer.Resolve<IToggleManager>();
			}
		}
	}
}
