using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public interface IIoCTestContext
	{
		void Reset();
		void Reset(Action<ISystem, IIocConfiguration> setup);
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
			QueryAllAttributes<ToggleAttribute>().ForEach(a => toggles.Enable(a.Toggle));
			QueryAllAttributes<ToggleOffAttribute>().ForEach(a => toggles.Disable(a.Toggle));
			return toggles;
		}

		protected virtual FakeConfigReader Config()
		{
			var config = new FakeConfigReader();
			QueryAllAttributes<SettingAttribute>().ForEach(x => config.FakeSetting(x.Setting, x.Value));
			return config;
		}

		protected IEnumerable<T> QueryAllAttributes<T>()
		{
			var fromFixture = _fixtureType.GetCustomAttributes(typeof (T), false).Cast<T>();
			var fromTest = _method.GetCustomAttributes(typeof (T), false).Cast<T>();
			var fromAttribute = GetType().GetCustomAttributes(typeof (T), false).Cast<T>();
			return fromFixture
				.Union(fromTest)
				.Union(fromAttribute)
				.ToArray();
		}

		protected virtual void Setup(ISystem system, IIocConfiguration configuration)
		{
		}

		protected virtual void Startup(IComponentContext container)
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
			Startup(_container);
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

		private void buildContainer(Action<ISystem, IIocConfiguration> setup)
		{
			var builder = new ContainerBuilder();
			var configReader = Config();
			var args = new IocArgs(configReader)
			{
				ThrottleMessages = false// the throttler shouldnt be started in ioc common at all, but...
			};
			var configuration = new IocConfiguration(args, Toggles());
			builder.RegisterModule(new CommonModule(configuration));
			builder.RegisterInstance(new MutableNow("2014-12-18 13:31")).As<INow>().AsSelf().SingleInstance();
			builder.RegisterType<FakeTime>().As<ITime>().AsSelf().SingleInstance();

			// maybe these shouldnt be faked after all.. maybe its just the principal that should handle it..
			builder.RegisterInstance(new FakeUserTimeZone(TimeZoneInfo.Utc)).As<IUserTimeZone>().AsSelf().SingleInstance();
			builder.RegisterInstance(new FakeUserCulture(CultureInfoFactory.CreateSwedishCulture())).As<IUserCulture>().AsSelf().SingleInstance();

			//don't check license for every test
			builder.RegisterType<SetNoLicenseActivator>().As<ISetLicenseActivator>().SingleInstance();

			builder.RegisterInstance(configReader).AsSelf().As<IConfigReader>();
			builder.RegisterInstance(this).As<IIoCTestContext>();

			var system = new ContainerBuilderWrapper(builder);
			Setup(system, configuration);
			if (_fixture is ISetup)
				(_fixture as ISetup).Setup(system, configuration);
			setup(system, configuration);

			_container = builder.Build();
		}

		private void disposeContainer()
		{
			_container.Dispose();
			_container = null;
		}

		private void injectMembers()
		{
			injectMembers(GetType(), this);
			injectMembers(_fixtureType, _fixture);
		}

		private void injectMembers(IReflect type, object instance)
		{
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(x => x.CanWrite);
			properties.ForEach(x => x.SetValue(instance, _container.Resolve(x.PropertyType), null));
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
			fields.ForEach(x => x.SetValue(instance, _container.Resolve(x.FieldType)));
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
			disposeContainer();
			buildContainer((b, c) => { });
			injectMembers();
		}

		public void Reset(Action<ISystem, IIocConfiguration> setup)
		{
			disposeContainer();
			buildContainer(setup);
			injectMembers();
		}

	}
}