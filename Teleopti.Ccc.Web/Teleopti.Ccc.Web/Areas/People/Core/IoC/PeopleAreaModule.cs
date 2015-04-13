using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Areas.Search.Controllers;

namespace Teleopti.Ccc.Web.Areas.People.Core.IoC
{
	public class PeopleAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PeopleSearchProvider>().As<IPeopleSearchProvider>();
		}
	}
}