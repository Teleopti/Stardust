using System.Web.Http;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Rta.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.Web.Core.Startup.Booter;


namespace Teleopti.Ccc.WebTest.Core.IoC
{
	[TestFixture]
	public class ContainerConfigurationTest
	{
		[Test]
		public void BootstrapperModuleHasBeenRegistered()
		{
			var builder = new ContainerConfiguration();
			using (var container = builder.Configure(string.Empty, new HttpConfiguration()))
			{
				container.IsRegistered<IBootstrapperTask>()
					.Should().Be.True();
			}
		}

		[Test]
		public void RepositoryModuleHasBeenRegistered()
		{
			var builder = new ContainerConfiguration();
			using (var container = builder.Configure(string.Empty, new HttpConfiguration()))
			{
				container.IsRegistered<IPersonRepository>()
					.Should().Be.True();
			}
		}

		[Test]
		public void MvcModuleHasBeenRegistered()
		{
			var builder = new ContainerConfiguration();
			using (var container = builder.Configure(string.Empty, new HttpConfiguration()))
			{
				container.IsRegistered<AuthenticationController>()
					.Should().Be.True();
			}
		}

		[Test]
		public void InitializeApplicationHasBeenRegistered()
		{
			var builder = new ContainerConfiguration();
			using (var container = builder.Configure(string.Empty, new HttpConfiguration()))
			{
				container.IsRegistered<IInitializeApplication>()
					.Should().Be.True();
			}
		}

		[Test]
		public void ApplicationDataHasBeenRegistered()
		{
			var builder = new ContainerConfiguration();
			using (var container = builder.Configure(string.Empty, new HttpConfiguration()))
			{
				container.IsRegistered<IApplicationData>()
					.Should().Be.True();
			}
		}

		[Test]
		public void StudentAvailabilityViewModelFactoryHasBeenRegistered()
		{
			var builder = new ContainerConfiguration();
			using (var container = builder.Configure(string.Empty, new HttpConfiguration()))
			{
				container.IsRegistered<IStudentAvailabilityViewModelFactory>()
					.Should().Be.True();
			}
		}

		[Test]
		public void ResourceHandlerModuleHasBeenRegistered()
		{
			var builder = new ContainerConfiguration();
			using (var container = builder.Configure(string.Empty, new HttpConfiguration()))
			{
				container.IsRegistered<IUserTextTranslator>()
					.Should().Be.True();
			}
		}

		[Test]
		public void ShouldResolveRtaController()
		{
			var builder = new ContainerConfiguration();
			using (var container = builder.Configure(string.Empty, new HttpConfiguration()))
			{
				container.Resolve<StateController>()
					.Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveForecastController()
		{
			var builder = new ContainerConfiguration();
			using (var container = builder.Configure(string.Empty, new HttpConfiguration()))
			{
				container.Resolve<ForecastController>()
					.Should().Not.Be.Null();
			}
		}
	}
}