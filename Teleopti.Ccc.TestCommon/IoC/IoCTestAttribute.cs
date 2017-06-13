using System;
using System.Collections.Generic;
using Autofac;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public interface IIoCTestContext
	{
		void SimulateRestart();
	};

	public interface INotCompatibleWithIoCTest
	{
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
	public class IoCTestAttribute : Attribute, ITestAction, IIoCTestContext
	{
		public ActionTargets Targets => ActionTargets.Test;

		private IContainer _container;
		private TestDoubles _testDoubles;
		private IoCTestService _service;
		private object _fixture;

		protected virtual FakeToggleManager Toggles()
		{
			return _service.Toggles();
		}

		protected virtual FakeConfigReader Config()
		{
			return _service.Config();
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

		public void BeforeTest(ITest testDetails)
		{
			_fixture = testDetails.Fixture;
			if (_fixture is INotCompatibleWithIoCTest)
				throw new NotSupportedException("This fixture (or base fixtures) is not compatible with a IoC test attributeDatabaseTest");
			_service = new IoCTestService(testDetails, this);
			buildContainer();
			Startup(_container);
			_service.InjectFrom(_container);
			BeforeTest();
		}

		public void AfterTest(ITest testDetails)
		{
			AfterTest();
			disposeContainer();
			disposeTestDoubles();
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
			setupBuilder(new ignoringTestDoubles(builder, null));
			_testDoubles.RegisterFromPreviousContainer(builder);
			_container = builder.Build();
		}

		private void disposeTestDoubles()
		{
			_testDoubles?.Dispose();
			_testDoubles = null;
		}

		private void setupBuilder(ISystem system)
		{
			var config = Config();
			var toggles = Toggles();
			(_fixture as IConfigureToggleManager)?.Configure(toggles);
			var args = new IocArgs(config);
			(_fixture as ISetupConfiguration)?.SetupConfiguration(args);
			var configuration = new IocConfiguration(args, toggles);

			system.AddModule(new CommonModule(configuration));

			var now = new MutableNow();
			now.Is("2014-12-18 13:31");
			system.UseTestDouble(now).For<INow>();
			system.UseTestDouble<FakeTime>().For<ITime>();
			system.UseTestDouble(config).For<IConfigReader>();
			// we really shouldnt inject this, but if we do, maybe its better its correct...
			system.UseTestDouble(toggles).For<IToggleManager>();

			// Test helpers
			system.AddService(this);
			system.AddService<ConcurrencyRunner>();

			Setup(system, configuration);
			(_fixture as ISetup)?.Setup(system, configuration);
		}
		
		private void disposeContainer()
		{
			_container?.Dispose();
			_container = null;
		}
		
		protected IEnumerable<T> QueryAllAttributes<T>()
		{
			return _service.QueryAllAttributes<T>();
		}

		protected object Resolve(Type targetType)
		{
			return _service.Resolve(targetType);
		}

		protected T Resolve<T>()
		{
			return _service.Resolve<T>();
		}

		public void SimulateRestart()
		{
			disposeContainer();
			rebuildContainer();
			_service.InjectFrom(_container);
		}

		private class ignoringTestDoubles : SystemImpl
		{
			public ignoringTestDoubles(ContainerBuilder builder, TestDoubles testDoubles) : base(builder, testDoubles)
			{
			}

			public override ITestDoubleFor UseTestDouble<TTestDouble>()
			{
				return new ignoreTestDouble();
			}

			public override ITestDoubleFor UseTestDouble<TTestDouble>(TTestDouble instance)
			{
				return new ignoreTestDouble();
			}
		}

		private class ignoreTestDouble : ITestDoubleFor
		{
			public void For<T>()
			{
			}

			public void For<T1, T2>()
			{
			}

			public void For<T1, T2, T3>()
			{
			}

			public void For<T1, T2, T3, T4>()
			{
			}

			public void For<T1, T2, T3, T4, T5>()
			{
			}

			public void For(Type type)
			{
			}
		}
	}

	public interface IConfigureToggleManager
	{
		void Configure(FakeToggleManager toggleManager);
	}
}