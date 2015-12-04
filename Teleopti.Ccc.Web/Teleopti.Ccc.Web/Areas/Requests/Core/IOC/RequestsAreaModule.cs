using Autofac;
using Teleopti.Ccc.Web.Areas.Requests.Core.Provider;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.IOC
{
	public class RequestsAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<RequestsProvider>().As<IRequestsProvider>().SingleInstance();
			builder.RegisterType<RequestsViewModelFactory>().As<IRequestsViewModelFactory>().SingleInstance();
		}
	}
}