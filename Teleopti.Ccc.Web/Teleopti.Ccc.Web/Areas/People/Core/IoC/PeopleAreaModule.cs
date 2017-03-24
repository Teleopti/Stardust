using Autofac;
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
			builder.RegisterType<MultipartHttpContentExtractor>().As<IMultipartHttpContentExtractor>().SingleInstance();
		
			
		}
	}
}