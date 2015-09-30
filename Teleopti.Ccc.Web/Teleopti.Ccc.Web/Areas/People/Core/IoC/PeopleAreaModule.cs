using Autofac;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Web.Areas.People.Core.Persisters;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;

namespace Teleopti.Ccc.Web.Areas.People.Core.IoC
{
	public class PeopleAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PeopleSearchProvider>().As<IPeopleSearchProvider>().SingleInstance();
			builder.RegisterType<PeoplePersister>().As<IPeoplePersister>().SingleInstance().ApplyAspects();
			builder.RegisterType<UserValidator>().As<IUserValidator>().SingleInstance();
			builder.RegisterType<PersonDataProvider>().As<IPersonDataProvider>().SingleInstance();
			builder.RegisterType<PersonInfoUpdater>().As<IPersonInfoUpdater>().SingleInstance();
		}
	}
}