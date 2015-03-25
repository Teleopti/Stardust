using System;
using System.Linq;
using System.Reflection;
using Autofac;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.MultipleConfig;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public interface IIoCTestContext
	{
		void Reset();
		void Reset(Action<ContainerBuilder, IIocConfiguration> registerInContainer);
	};

	public interface IRegisterInContainer
	{
		void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration);
	};

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
	public class IoCTestAttribute : Attribute, ITestAction, IIoCTestContext
	{
		public ActionTargets Targets
		{
			get { return ActionTargets.Test; }
		}

		private IContainer _container;
		private object _fixture;
		private Type _fixtureType;
		private MethodInfo _method;

		protected virtual FakeToggleManager Toggles()
		{
			var toggles = new FakeToggleManager();

			var enableTogglesFixture = _fixtureType.GetCustomAttributes(typeof(ToggleAttribute), false).Cast<ToggleAttribute>();
			var enableTogglesTest = _method.GetCustomAttributes(typeof(ToggleAttribute), false).Cast<ToggleAttribute>();
			var enableToggles = enableTogglesFixture.Union(enableTogglesTest);

			enableToggles.ForEach(a => toggles.Enable(a.Toggle));

			var disableTogglesFixture = _fixtureType.GetCustomAttributes(typeof(ToggleOffAttribute), false).Cast<ToggleOffAttribute>();
			var disableTogglesTest = _method.GetCustomAttributes(typeof(ToggleOffAttribute), false).Cast<ToggleOffAttribute>();
			var disableToggles = disableTogglesFixture.Union(disableTogglesTest);

			disableToggles.ForEach(a => toggles.Disable(a.Toggle));

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
			method(testDetails);
			buildContainer((b, c) => { });
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

		private void method(TestDetails testDetails)
		{
			_method = testDetails.Method;
		}

		private void buildContainer(Action<ContainerBuilder, IIocConfiguration> registerInContainer)
		{
			var builder = new ContainerBuilder();
			var configuration = new IocConfiguration(new IocArgs(new AppConfigReader()) {ClearCache = true}, Toggles());
			builder.RegisterModule(new CommonModule(configuration));
			builder.RegisterInstance(new MutableNow("2014-12-18 13:31")).As<INow>().AsSelf();
			builder.RegisterInstance(new FakeUserTimeZone(TimeZoneInfo.Utc)).As<IUserTimeZone>().AsSelf().SingleInstance();
			builder.RegisterInstance(new FakeUserCulture(CultureInfoFactory.CreateSwedishCulture())).As<IUserCulture>().AsSelf().SingleInstance();
			builder.RegisterInstance(this).As<IIoCTestContext>();
			RegisterInContainer(builder, configuration);
			if (_fixture is IRegisterInContainer)
				(_fixture as IRegisterInContainer).RegisterInContainer(builder, configuration);
			registerInContainer(builder, configuration);
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



		public void Reset()
		{
			buildContainer((b, c) => { });
			injectMembers();
		}

		public void Reset(Action<ContainerBuilder, IIocConfiguration> registerInContainer)
		{
			buildContainer(registerInContainer);
			injectMembers();
		}

	}
}