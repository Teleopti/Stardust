using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Tracking;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class PersonAccountModule : Module
	{
		private readonly IocConfiguration _configuration;

		public PersonAccountModule(IocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
            builder.RegisterType<TraceableRefreshService>().As<ITraceableRefreshService>();
            builder.RegisterType<CalculatePersonAccount>();
			builder.RegisterType<PersonAccountUpdater>().As<IPersonAccountUpdater>();
			if (!_configuration.IsToggleEnabled(Toggles.WFM_Clear_Data_After_Leaving_Date_47768))
			{
				builder.RegisterType<ClearPersonRelatedInformation>().As<IPersonLeavingUpdater>().SingleInstance();
			}
			else
			{
				// IPersonLeavingUpdater should be removed when toggle WFM_Clear_Data_After_Leaving_Date_47768 removed.
				// By now we just apply the dummy implementation for it.
				builder.RegisterType<DummyPersonLeavingUpdater>().As<IPersonLeavingUpdater>().SingleInstance();
			}
		}
	}
}