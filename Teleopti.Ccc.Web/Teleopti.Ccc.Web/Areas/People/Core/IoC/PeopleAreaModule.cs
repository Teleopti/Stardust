using Autofac;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Ccc.Web.Areas.People.Core.Aspects;
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
			builder.RegisterType<AuditHelper>().As<IAuditHelper>().SingleInstance();
			builder.RegisterType<RoleManager>().As<IRoleManager>().SingleInstance().ApplyAspects();
			builder.RegisterType<AuditPersonAspect>().As<IAspect>().SingleInstance();
			builder.RegisterType<PersonFinderReadOnlyRepository>().As<IPersonFinderReadOnlyRepository>().SingleInstance();
		}
	}
}