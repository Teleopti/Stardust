using Autofac;
using Teleopti.Ccc.Domain.AgentInfo.ImportAgent;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Web.Areas.People.Core.Persisters;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;

namespace Teleopti.Ccc.Web.Areas.People.Core.IoC
{
	public class PeopleAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PeopleSearchProvider>().As<IPeopleSearchProvider>().SingleInstance();
			builder.RegisterType<PersonDataProvider>().As<IPersonDataProvider>().SingleInstance();
			builder.RegisterType<PersonInfoUpdater>().As<IPersonInfoUpdater>().SingleInstance();
			builder.RegisterType<ImportAgentFileValidator>().As<IImportAgentFileValidator>().SingleInstance();
			builder.RegisterType<TenantUserPersister>().As<ITenantUserPersister>().SingleInstance().ApplyAspects();
			builder.RegisterType<ImportAgentDataProvider>().As<IImportAgentDataProvider>().SingleInstance();
			builder.RegisterType<AgentPersister>().As<IAgentPersister>().SingleInstance();
			builder.RegisterType<FileProcessor>().As<IFileProcessor>().SingleInstance();
			builder.RegisterType<MultipartHttpContentExtractor>().As<IMultipartHttpContentExtractor>().SingleInstance();
			builder.RegisterType<WorkbookHandler>().As<IWorkbookHandler>().SingleInstance();
		}
	}
}