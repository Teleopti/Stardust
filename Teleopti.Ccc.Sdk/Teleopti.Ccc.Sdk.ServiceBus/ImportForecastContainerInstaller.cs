﻿using Autofac;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class ImportForecastContainerInstaller : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ForecastsFileContentProvider>().As<IForecastsFileContentProvider>();
        }
    }
}
