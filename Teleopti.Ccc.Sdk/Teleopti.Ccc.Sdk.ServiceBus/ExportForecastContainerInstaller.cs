using System;
using Autofac;
using Teleopti.Ccc.Domain.Forecasting.Export;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class ExportForecastContainerInstaller : Module
    {
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SaveForecastToSkillCommand>().As<ISaveForecastToSkillCommand>();
			builder.RegisterType<MultisiteForecastToSkillCommand>().As<IMultisiteForecastToSkillCommand>();
			builder.RegisterType<OpenAndSplitSkillCommand>().As<IOpenAndSplitSkillCommand>();
		}
    }
}