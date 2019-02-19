using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Autofac;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.TestCommon.IoC
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	[Toggle(Domain.FeatureFlags.Toggles.MessageBroker_HttpSenderThrottleRequests_79140)]
	[Toggle(Domain.FeatureFlags.Toggles.MessageBroker_VeganBurger_79140)]
	[Toggle(Domain.FeatureFlags.Toggles.MessageBroker_ServerThrottleMessages_79140)]
	[Toggle(Domain.FeatureFlags.Toggles.MessageBroker_ScheduleChangedMessagePackaging_79140)]
	public class IoCTestAttribute : Attribute, ITestAction, IIoCTestContext
	{
		public ActionTargets Targets => ActionTargets.Test;

		private IContainer _container;
		private TestDoubles _testDoubles;
		private IoCTestService _service;
		private object _fixture;

		public MutableNow Now;
		
		protected virtual FakeToggleManager Toggles()
		{
			return _service.Toggles();
		}

		protected virtual FakeConfigReader Config()
		{
			return _service.Config();
		}

		protected virtual void Extend(IExtend extend, IocConfiguration configuration)
		{
		}

		protected virtual void Isolate(IIsolate isolate)
		{
		}

		protected virtual void BeforeInject(IComponentContext container)
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
			BeforeInject(_container);
			_service.InjectFrom(_container);
			
			Now.Is("2014-12-18 13:31");
			
			BeforeTest();
			(_fixture as ITestInterceptor)?.OnBefore();
		}

		public void AfterTest(ITest testDetails)
		{
			AfterTest();
			disposeContainer();
			cleanupInstances();
			disposeTestDoubles();
		}

		private void cleanupInstances()
		{
			_service = null;
			Now = null;
		}

		private void buildContainer()
		{
			var builder = new ContainerBuilder();
			_testDoubles = new TestDoubles();
			var s = new SystemImpl(builder, _testDoubles);
			setupBuilder(s, s);
			_container = builder.Build();
		}

		private void rebuildContainer()
		{
			var builder = new ContainerBuilder();
			var s = new ignoringTestDoubles(builder, null);
			setupBuilder(s, s);
			_testDoubles.RegisterFromPreviousContainer(builder);
			_container = builder.Build();
		}

		private void disposeTestDoubles()
		{
			_testDoubles?.Dispose();
			_testDoubles = null;
		}

		private void setupBuilder(IExtend extend, IIsolate isolate)
		{
			var config = Config();
			var toggles = Toggles();
			(_fixture as IConfigureToggleManager)?.Configure(toggles);
			var args = new IocArgs(config);
			if (QueryAllAttributes<UseIocForFatClientAttribute>().Any())
				args.IsFatClient = true;
			(_fixture as ISetupConfiguration)?.SetupConfiguration(args);
			args.EnableLegacyServiceLocators = !TestContext.CurrentContext.isParallel();
			var configuration = new IocConfiguration(args, toggles);

			extend.AddModule(new CommonModule(configuration));
			extend.AddModule(new DataFactoryModule());

			Extend(extend, configuration);
			QueryAllExtensions<IExtendSystem>()
				.ForEach(x => x.Extend(extend, configuration));

			isolate.UseTestDouble<MutableNow>().For<INow, IMutateNow>();
			isolate.UseTestDouble<FakeTime>().For<ITime>();
			isolate.UseTestDouble(config).For<IConfigReader>();
			// we really shouldnt inject this, but if we do, maybe its better its correct...
			isolate.UseTestDouble(toggles).For<IToggleManager>();

			// Test helpers
			extend.AddService(this); // move up
			extend.AddService<ConcurrencyRunner>(); // move to TestModule

			Isolate(isolate);
			QueryAllExtensions<IIsolateSystem>()
				.ForEach(x => x.Isolate(isolate));
		}

		private void disposeContainer()
		{
			_service?.Dispose();
			_container?.Dispose();
			_container = null;
			_fixture = null;
		}

		protected IEnumerable<T> QueryAllExtensions<T>() where T : class =>
			QueryAllAttributes<Attribute>().OfType<T>()
				.Append(_fixture as T)
				.Where(x => x != null);

		protected IEnumerable<T> QueryAllAttributes<T>() where T : Attribute =>
			_service.QueryAllAttributes<T>();

		protected object Resolve(Type targetType)
		{
			return _service.Resolve(targetType);
		}

		protected T Resolve<T>()
		{
			return _service.Resolve<T>();
		}

		public void SimulateShutdown()
		{
			disposeContainer();
		}

		public void SimulateRestart()
		{
			disposeContainer();
			rebuildContainer();
			_service.InjectFrom(_container);
		}

		public void SimulateNewRequest()
		{
			var scope = _container.BeginLifetimeScope();
			_service.InjectFrom(scope);
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