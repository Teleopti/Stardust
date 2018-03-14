using Autofac;
using System.Web.Http;

namespace Teleopti.Ccc.Web.Core.IoC
{
	public interface IContainerConfiguration
	{
		ILifetimeScope Configure(string featureTogglePath, HttpConfiguration httpConfiguration);
	}
}
