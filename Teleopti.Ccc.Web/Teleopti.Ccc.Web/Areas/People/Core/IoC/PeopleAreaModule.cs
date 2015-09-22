using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Web.Areas.People.Core.Persisters;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Areas.Search.Controllers;

namespace Teleopti.Ccc.Web.Areas.People.Core.IoC
{
	public class PeopleAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PeopleSearchProvider>().As<IPeopleSearchProvider>();
			builder.RegisterType<PeoplePersister>().As<IPeoplePersister>().SingleInstance().ApplyAspects();
			builder.RegisterType<UserValidator>().As<IUserValidator>();
			builder.RegisterType<PersonDataProvider>().As<IPersonDataProvider>();
			builder.RegisterType<PersonInfoUpdater>().As<IPersonInfoUpdater>();
		}
	}
}