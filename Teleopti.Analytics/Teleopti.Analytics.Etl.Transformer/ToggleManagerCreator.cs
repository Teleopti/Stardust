using Autofac;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Analytics.Etl.Transformer
{
	public class ToggleManagerCreator : IToggleManagerCreator
	{
		public IToggleManager Create(string togglePath)
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
