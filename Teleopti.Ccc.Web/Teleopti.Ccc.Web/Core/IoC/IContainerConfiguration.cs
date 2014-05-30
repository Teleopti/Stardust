using Autofac;

namespace Teleopti.Ccc.Web.Core.IoC
{
	public interface IContainerConfiguration
	{
		IContainer Configure(string featureTogglePath);
	}
}
