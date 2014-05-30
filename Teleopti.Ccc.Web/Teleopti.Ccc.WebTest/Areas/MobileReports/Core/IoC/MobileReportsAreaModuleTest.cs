using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.MobileReports.Core;
using Teleopti.Ccc.Web.Areas.MobileReports.Core.IoC;
using Teleopti.Ccc.Web.Areas.MobileReports.Core.Matrix;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.MobileReports.Core.IoC
{
	using Teleopti.Ccc.Web.Areas.MobileReports.Core.Providers;

	[TestFixture]
	public class MobileReportsAreaModuleTest
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			_container = new ContainerConfiguration().Configure(string.Empty);
			var containerAdder = new ContainerBuilder();
			containerAdder.RegisterModule(new MobileReportsAreaModule());
			containerAdder.Update(_container);
		}

		[TearDown]
		public void Teardown()
		{
			if (_container != null)
				_container.Dispose();
		}

		#endregion

		private IContainer _container;

		[Test]
		public void ShouldRegisterDateBoxGlobalizationViewModelFactory()
		{
			using (var scope = _container.BeginLifetimeScope())
			{
				scope.IsRegistered<IDateBoxGlobalizationViewModelFactory>()
					.Should().Be.True();
			}
		}

		[Test]
		public void ShouldRegisterDefinedReportProvider()
		{
			using (var scope = _container.BeginLifetimeScope())
			{
				scope.IsRegistered<IDefinedReportProvider>()
					.Should().Be.True();
			}
		}

		[Test]
		public void ShouldRegisterIReportDataService()
		{
			using (var scope = _container.BeginLifetimeScope())
			{
				scope.IsRegistered<IReportDataService>()
					.Should().Be.True();
			}
		}

		[Test]
		public void ShouldRegisterIReportRequestValidator()
		{
			using (var scope = _container.BeginLifetimeScope())
			{
				scope.IsRegistered<IReportRequestValidator>()
					.Should().Be.True();
			}
		}

		[Test]
		public void ShouldRegisterISkillProvider()
		{
			using (var scope = _container.BeginLifetimeScope())
			{
				scope.IsRegistered<ISkillProvider>()
					.Should().Be.True();
			}
		}

		[Test]
		public void ShouldRegisterIUserInfoProvider()
		{
			using (var scope = _container.BeginLifetimeScope())
			{
				scope.IsRegistered<IWebReportUserInfoProvider>()
					.Should().Be.True();
			}
		}

		[Test]
		public void ShouldRegisterMobileReportsViewModelFactory()
		{
			using (var scope = _container.BeginLifetimeScope())
			{
				scope.IsRegistered<IReportViewModelFactory>()
					.Should().Be.True();
			}
		}


		[Test]
		public void ShouldRegisterUserTextTranslator()
		{
			using (var scope = _container.BeginLifetimeScope())
			{
				scope.IsRegistered<IUserTextTranslator>()
					.Should().Be.True();
			}
		}
	}
}