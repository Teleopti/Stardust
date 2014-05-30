using System;
using System.Threading.Tasks;
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
		[SetUp]
		public void Setup()
		{
			ApplicationStartModule.ErrorAtStartup = null;
			ApplicationStartModule.TasksFromStartup = null;
		}

		[TearDown]
		public void TearDown()
		{
			ApplicationStartModule.ErrorAtStartup = null;
			ApplicationStartModule.TasksFromStartup = null;
		}

		[Test]
		public void ShouldOnlyRunOncePerModuleType()
		{
			var httpApplication =  MockRepository.GenerateMock<HttpApplication>();
			var bootstrapper = MockRepository.GenerateMock<IBootstrapper>();
			var target = new ApplicationStartModule();
			var target2 = new ApplicationStartModule();
			target.InjectForTest(bootstrapper, new containerConfForBootstrapperTasks(new List<IBootstrapperTask>()));

			target.Init(httpApplication);
			target2.Init(httpApplication);

			bootstrapper.AssertWasCalled(x => x.Run(null), o => o.IgnoreArguments().Repeat.Times(1));
		}

		[Test]
		public void ShouldRecordException()
		{
			var httpApplication = MockRepository.GenerateMock<HttpApplication>();
			var bootstrapper = MockRepository.GenerateMock<IBootstrapper>();
			var target = new ApplicationStartModule();
			target.InjectForTest(bootstrapper, new containerConfForBootstrapperTasks(new List<IBootstrapperTask>()));
			var ex = new Exception();
			bootstrapper.Stub(x => x.Run(null)).IgnoreArguments().Throw(ex);

			target.Init(httpApplication);

			ApplicationStartModule.ErrorAtStartup.Should().Be.SameInstanceAs(ex);
		}

		[Test]
		public void ShouldRecordTasksReturned()
		{
			var httpApplication = MockRepository.GenerateMock<HttpApplication>();
			var bootstrapper = MockRepository.GenerateMock<IBootstrapper>();
			var tasks = new[] {new Task(() => { })};
			bootstrapper.Stub(x => x.Run(null)).IgnoreArguments().Return(tasks);

			var target = new ApplicationStartModule();
			target.InjectForTest(bootstrapper, new containerConfForBootstrapperTasks(new List<IBootstrapperTask>()));

			target.Init(httpApplication);

			ApplicationStartModule.TasksFromStartup.Should().Have.SameValuesAs(tasks);
		}

		[Test]
		public void ShouldDoNothingAtDispose()
		{
			var target = new ApplicationStartModule();
			var target2 = new ApplicationStartModule();
			target.Dispose();
			target2.Dispose();
		}

		private class containerConfForBootstrapperTasks : IContainerConfiguration
		{
			public containerConfForBootstrapperTasks(IEnumerable<IBootstrapperTask> tasks)
			{
				Tasks = tasks;
			}

			public IContainer Configure(string featureTogglePath)
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