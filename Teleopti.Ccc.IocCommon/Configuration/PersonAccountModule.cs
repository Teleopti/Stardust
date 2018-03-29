using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Tracking;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class PersonAccountModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public PersonAccountModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
            builder.RegisterType<TraceableRefreshService>().As<ITraceableRefreshService>();
            builder.RegisterType<CalculatePersonAccount>();
			builder.RegisterType<PersonAccountUpdater>().As<IPersonAccountUpdater>();
			if (!_configuration.Toggle(Toggles.WFM_Clear_Data_After_Leaving_Date_47768))
			{
				builder.RegisterType<ClearPersonRelatedInformation>().As<IPersonLeavingUpdater>().SingleInstance();
			}

		}
	}
}