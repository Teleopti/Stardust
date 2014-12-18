using System;
using System.Linq;
using System.Reflection;
using Autofac;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class IoCTestAttribute : Attribute, ITestAction
	{
		public ActionTargets Targets
		{
			get { return ActionTargets.Test; }
		}

		private IContainer _container;
		private object _fixture;
		private Type _fixtureType;

		protected virtual IToggleManager Toggles()
		{
			var attributes = _fixtureType.GetCustomAttributes(typeof(ToggleAttribute), false).Cast<ToggleAttribute>();
			var toggles = new FakeToggleManager();
			attributes.ForEach(a => toggles.Enable(a.Toggle));
			return toggles;
		}

		protected virtual void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
		}

		protected virtual void BeforeTest()
		{
		}

		protected virtual void AfterTest()
		{
		}

		public void BeforeTest(TestDetails testDetails)
		{
			fixture(testDetails);
			buildContainer();
			injectMembers();
			BeforeTest();
		}

		public void AfterTest(TestDetails testDetails)
		{
			AfterTest();
			disposeContainer();
		}

		private void fixture(TestDetails testDetails)
		{
			_fixture = testDetails.Fixture;
			_fixtureType = _fixture.GetType();
		}

		private void buildContainer()
		{
			var builder = new ContainerBuilder();
			var configuration = new IocConfiguration(new IocArgs(), Toggles());
			builder.RegisterModule(new CommonModule(configuration));
			RegisterInContainer(builder, configuration);
			_container = builder.Build();
		}

		private void disposeContainer()
		{
			_container.Dispose();
		}

		private void injectMembers()
		{
			var properties = _fixtureType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			properties.ForEach(x => x.SetValue(_fixture, _container.Resolve(x.PropertyType), null));
			var fields = _fixtureType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			fields.ForEach(x => x.SetValue(_fixture, _container.Resolve(x.FieldType)));
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