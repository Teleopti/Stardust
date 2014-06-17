using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Web.Areas.MobileReports.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.IoC
{
	[TestFixture]
	public class ContainerConfigurationTest
	{
		private IContainerConfiguration builder;

		[SetUp]
		public void Setup()
		{
			builder = new ContainerConfiguration();
		}

		[Test]
		public void BootstrapperModuleHasBeenRegistered()
		{
			using (var container = builder.Configure(string.Empty))
			{
				container.IsRegistered<IBootstrapperTask>()
					.Should().Be.True();
			}
		}

		[Test]
		public void RepositoryModuleHasBeenRegistered()
		{
			using (var container = builder.Configure(string.Empty))
			{
				container.IsRegistered<IPersonRepository>()
					.Should().Be.True();
			}
		}

		[Test]
		public void MvcModuleHasBeenRegistered()
		{
			using (var container = builder.Configure(string.Empty))
			{
				container.IsRegistered<AuthenticationController>()
					.Should().Be.True();
			}
		}

		[Test]
		public void InitializeApplicationHasBeenRegistered()
		{
			using (var container = builder.Configure(string.Empty))
			{
				container.IsRegistered<IInitializeApplication>()
					.Should().Be.True();
			}
		}

		[Test]
		public void ApplicationDataHasBeenRegistered()
		{
			using (var container = builder.Configure(string.Empty))
			{
				container.IsRegistered<IApplicationData>()
					.Should().Be.True();
			}
		}

		[Test]
		public void StudentAvailabilityViewModelFactoryHasBeenRegistered()
		{
			using (var container = builder.Configure(string.Empty))
			{
				container.IsRegistered<IStudentAvailabilityViewModelFactory>()
					.Should().Be.True();
			}
		}

		[Test]
		public void ShouldResolveNumberOfAgentsInTeamReader()
		{
			using (var container = builder.Configure(string.Empty))
			{
				container.Resolve<INumberOfAgentsInTeamReader>()
					.Should().Not.Be.Null();
			}
		}

		[Test]
		public void ResourceHandlerModuleHasBeenRegistered()
		{
			using (var container = builder.Configure(string.Empty))
			{
				container.IsRegistered<IUserTextTranslator>()
					.Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveRtaController()
		{
			using (var container = builder.Configure(string.Empty))
			{
				container.Resolve<RtaServiceController>()
					.Should().Not.Be.Null();
			}
		}
	}
}
