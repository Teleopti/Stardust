using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class PeopleAreaModule : Module
	{
		private readonly IocConfiguration _iocConfiguration;

		public PeopleAreaModule(IocConfiguration iocConfiguration)
		{
			_iocConfiguration = iocConfiguration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PersonInfoHelper>().As<IPersonInfoHelper>().SingleInstance();
			registerType<IPersistPersonInfo, PersistPersonInfoWithAuditTrail, PersistPersonInfo>(builder,
				Toggles.Wfm_AuditTrail_GenericAuditTrail_74938);

			builder.RegisterType<ImportAgentFileValidator>().As<IImportAgentFileValidator>().SingleInstance();
			builder.RegisterType<ImportAgentDataProvider>().As<IImportAgentDataProvider>().SingleInstance();
			builder.RegisterType<AgentPersister>().As<IAgentPersister>().SingleInstance();
			builder.RegisterType<FileProcessor>().As<IFileProcessor>().SingleInstance();
			builder.RegisterType<WorkbookHandler>().As<IWorkbookHandler>().SingleInstance();
			builder.RegisterType<ImportAgentJobService>().As<IImportAgentJobService>();
		}

		private void registerType<T, TToggleOn, TToggleOff>(ContainerBuilder builder, Toggles toggle)
			where TToggleOn : T
			where TToggleOff : T
		{
			if (_iocConfiguration.IsToggleEnabled(toggle))
			{
				builder.RegisterType<TToggleOn>().As<T>().SingleInstance().ApplyAspects();
			}
			else
			{
				builder.RegisterType<TToggleOff>().As<T>().SingleInstance().ApplyAspects();
			}
		}
	}
}