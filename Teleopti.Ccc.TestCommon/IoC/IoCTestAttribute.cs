using System;
using Autofac;
using NUnit.Framework;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class IoCTestAttribute : Attribute, ITestAction
	{
		public ActionTargets Targets { get { return ActionTargets.Test; } }

		private IContainer _container;

		protected virtual void RegisterInContainer(ContainerBuilder builder)
		{
		}

		protected virtual void BeforeTest2()
		{
		}

		protected virtual void AfterTest2()
		{
		}

		public void BeforeTest(TestDetails testDetails)
		{
			buildContainer();
			resolveAndSetTarget(testDetails);
			BeforeTest2();
		}

		public void AfterTest(TestDetails testDetails)
		{
			AfterTest2();
			disposeContainer();
		}

		private void buildContainer()
		{
			var builder = new ContainerBuilder();
			var iocConfiguration = new IocConfiguration(new IocArgs(), new FalseToggleManager());
			builder.RegisterModule(new CommonModule(iocConfiguration));
			RegisterInContainer(builder);
			_container = builder.Build();
		}

		private void disposeContainer()
		{
			_container.Dispose();
		}

		private void resolveAndSetTarget(TestDetails testDetails)
		{
			dynamic fixture = testDetails.Fixture;
			var targetType = testDetails.Fixture.GetType().GetProperty("Target").PropertyType;
			// "as dynamic" is actually required, even though resharper says differently
			fixture.Target = Resolve(targetType) as dynamic;
		}

		protected object Resolve(Type targetType)
		{
			return _container.Resolve(targetType);
		}

		protected T Resolve<T>()
		{
			return _container.Resolve<T>();
		}
	}
}