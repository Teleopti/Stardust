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
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public interface IIoCTestContext
	{
		void SimulateRestart();
	};

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
	public class IoCTestAttribute : Attribute, ITestAction, IIoCTestContext
	{
		public ActionTargets Targets
		{
			get { return ActionTargets.Test; }
		}

		private IContainer _container;
		private TestDoubles _testDoubles;
		private object _fixture;
		private Type _fixtureType;
		private MethodInfo _method;

		protected virtual FakeToggleManager Toggles()
		{
			var toggles = new FakeToggleManager();
			if (QueryAllAttributes<AllTogglesOnAttribute>().Any())
				toggles.EnableAll();
			if (QueryAllAttributes<AllTogglesOffAttribute>().Any())
				toggles.DisableAll();
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
			buildContainer();
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

		private void buildContainer()
		{
			var builder = new ContainerBuilder();
			_testDoubles = new TestDoubles();
			setupBuilder(new SystemImpl(builder, _testDoubles));
			_container = builder.Build();
		}

		private void rebuildContainer()
		{
			var builder = new ContainerBuilder();
			setupBuilder(new IgnoringTestDoubles(builder, null));
			_testDoubles.RegisterFromPreviousContainer(builder);
			_container = builder.Build();
		}

		private void setupBuilder(ISystem system)
		{
			var configReader = Config();
			var toggleManager = Toggles();
			var args = new IocArgs(configReader)
			{
				ThrottleMessages = false // the throttler shouldnt be started in ioc common at all, but...
			};
			if (_fixture is ISetupConfiguration)
				(_fixture as ISetupConfiguration).SetupConfiguration(args);
			var configuration = new IocConfiguration(args, toggleManager);
			
			system.AddModule(new CommonModule(configuration));

			system.UseTestDouble(new MutableNow("2014-12-18 13:31")).For<INow>();
			system.UseTestDouble<FakeTime>().For<ITime>();

			// maybe these shouldnt be faked after all.. maybe its just the principal that should handle it..
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			system.UseTestDouble(new FakeUserCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserCulture>();

			//don't check license for every test
			system.UseTestDouble<SetNoLicenseActivator>().For<ISetLicenseActivator>();

			system.UseTestDouble(configReader).For<IConfigReader>();
			// we really shouldnt inject this, but if we do, maybe its better its correct...
			system.UseTestDouble(toggleManager).For<IToggleManager>();

			system.AddService(this);

			Setup(system, configuration);
			if (_fixture is ISetup)
				(_fixture as ISetup).Setup(system, configuration);
		}

		private void disposeContainer()
		{
			if (_container != null)
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

		public void SimulateRestart()
		{
			disposeContainer();
			rebuildContainer();
			injectMembers();
		}

	}
}