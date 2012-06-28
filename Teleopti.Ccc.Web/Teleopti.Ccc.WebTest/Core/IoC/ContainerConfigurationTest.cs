using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.MobileReports.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
			using (var container = builder.Configure())
			{
				container.IsRegistered<IBootstrapperTask>()
					.Should().Be.True();
			}
		}

		[Test]
		public void RepositoryModuleHasBeenRegistered()
		{
			using (var container = builder.Configure())
			{
				container.IsRegistered<IPersonRepository>()
					.Should().Be.True();
			}
		}

		[Test]
		public void UnitOfWorkModuleHasBeenRegistered()
		{
			using (var container = builder.Configure())
			{
				container.IsRegistered<IUnitOfWorkFactory>()
					.Should().Be.True();
			}
		}


		[Test]
		public void MvcModuleHasBeenRegistered()
		{
			using (var container = builder.Configure())
			{
				container.IsRegistered<AuthenticationController>()
					.Should().Be.True();
			}
		}

		[Test]
		public void InitializeApplicationHasBeenRegistered()
		{
			using (var container = builder.Configure())
			{
				container.IsRegistered<IInitializeApplication>()
					.Should().Be.True();
			}
		}

		[Test]
		public void ApplicationDataHasBeenRegistered()
		{
			using (var container = builder.Configure())
			{
				container.IsRegistered<IApplicationData>()
					.Should().Be.True();
			}
		}

		[Test]
		public void StudentAvailabilityViewModelFactoryHasBeenRegistered()
		{
			using (var container = builder.Configure())
			{
				container.IsRegistered<IStudentAvailabilityViewModelFactory>()
					.Should().Be.True();
			}
		}

		[Test]
		public void MobileReportsAreaModuleHasBeenLoaded()
		{
			using (var container = builder.Configure())
			{
				container.IsRegistered<IReportViewModelFactory>()
					.Should().Be.True();
			}
		}
	}
}
