using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Persisters.Auditing;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class PeopleAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PersonInfoHelper>().As<IPersonInfoHelper>().SingleInstance();
			builder.RegisterType<PersistPersonInfo>().As<IPersistPersonInfo>().SingleInstance();
			builder.RegisterType<ImportAgentFileValidator>().As<IImportAgentFileValidator>().SingleInstance();
			builder.RegisterType<TenantUserPersister>().As<ITenantUserPersister>().SingleInstance().ApplyAspects();
			builder.RegisterType<ImportAgentDataProvider>().As<IImportAgentDataProvider>().SingleInstance();
			builder.RegisterType<AgentPersister>().As<IAgentPersister>().SingleInstance();
			builder.RegisterType<FileProcessor>().As<IFileProcessor>().SingleInstance();
			builder.RegisterType<WorkbookHandler>().As<IWorkbookHandler>().SingleInstance();
			builder.RegisterType<ImportAgentJobService>().As<IImportAgentJobService>();
			builder.RegisterType<ImportJobArtifactValidator>().As<IImportJobArtifactValidator>().ApplyAspects();
			builder.RegisterType<PersonAccessPersister>().As<IPersonAccessPersister>().SingleInstance().ApplyAspects();
		}
	}
}