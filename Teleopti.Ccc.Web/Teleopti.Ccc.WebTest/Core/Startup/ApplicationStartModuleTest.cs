using System;
using System.Web;
using System.Collections.Generic;
using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.Web.Core.Startup;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.WebTest.Core.Startup
{
	[TestFixture]
	public class ApplicationStartModuleTest
	{
		private ApplicationStartModule target;
		private ApplicationStartModule target2;
		private IBootstrapper _bootstrapper;
		private containerConfForBootstrapperTasks containerConfiguration;
		private MockRepository mocks;
		private HttpApplication httpApplication;


		[SetUp]
		public void Startup()
		{
			target = new ApplicationStartModule();
			target2 = new ApplicationStartModule();
			mocks = new MockRepository();
			_bootstrapper = mocks.StrictMock<IBootstrapper>();
			containerConfiguration = new containerConfForBootstrapperTasks(new List<IBootstrapperTask>());
			target.HackForTest(_bootstrapper, containerConfiguration);
			target2.HackForTest(_bootstrapper, containerConfiguration);
			httpApplication = mocks.DynamicMock<HttpApplication>();
		}

		[Test]
		public void ShouldOnlyRunOncePerModuleType()
		{
			using (mocks.Record())
			{
				_bootstrapper.Run(containerConfiguration.Tasks);
			}
			using (mocks.Playback())
			{
				target.Init(httpApplication);
				target2.Init(httpApplication);
			}
			ApplicationStartModule.ErrorAtStartup
				.Should().Be.Null();
		}

		[Test]
		public void ShouldRecordException()
		{
			var ex = new Exception();
			using (mocks.Record())
			{
				_bootstrapper.Run(containerConfiguration.Tasks);
				LastCall.Throw(ex);
			}
			using (mocks.Playback())
			{
				target.Init(httpApplication);
				target2.Init(httpApplication); //should do nothing
			}
			ApplicationStartModule.ErrorAtStartup
				.Should().Be.SameInstanceAs(ex);
		}

		[Test]
		public void ShouldDoNothingAtDispose()
		{
			target.Dispose();
			target2.Dispose();
		}

		private class containerConfForBootstrapperTasks : IContainerConfiguration
		{
			public containerConfForBootstrapperTasks(IEnumerable<IBootstrapperTask> tasks)
			{
				Tasks = tasks;
			}

			public IContainer Configure()
			{
				Tasks = new List<IBootstrapperTask>();
				var builder = new ContainerBuilder();
				builder.Register(c => Tasks).As<IEnumerable<IBootstrapperTask>>();
				return builder.Build();
			}

			public IEnumerable<IBootstrapperTask> Tasks { get; private set; }
		}
	}
}