using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class ExportForecastContainerInstaller : Module
    {
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SaveForecastToSkillCommand>().As<ISaveForecastToSkillCommand>();
			builder.RegisterType<MultisiteForecastToSkillCommand>().As<IMultisiteForecastToSkillCommand>();
			builder.RegisterType<OpenAndSplitSkillCommand>().As<IOpenAndSplitSkillCommand>();
            builder.RegisterType<ImportForecastToSkillCommand>().As<IImportForecastToSkillCommand>();
            builder.RegisterType<SendImportForecastBusMessage>().As<ISendBusMessage>();
		}
    }
}