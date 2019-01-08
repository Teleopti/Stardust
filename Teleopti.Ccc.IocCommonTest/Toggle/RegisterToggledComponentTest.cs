using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	[DomainTest]
	[TestFixture(true)]
	[TestFixture(false)]
	public class RegisterToggledComponentTest : IExtendSystem
	{
		private readonly bool _singleInstance;
		public FakeToggleManager ToggleManager;
		public IMyService MyService;
		public ILifetimeScope LifetimeScope;

		public RegisterToggledComponentTest(bool singleInstance)
		{
			_singleInstance = singleInstance;
		}
		
		private class extraComponentsInTest : Module
		{
			private readonly bool _singleInstance;

			public extraComponentsInTest(bool singleInstance)
			{
				_singleInstance = singleInstance;
			}
			
			protected override void Load(ContainerBuilder builder)
			{
				var registration = builder.RegisterToggledComponent<MyServiceOn, MyServiceOff, IMyService>(Toggles.TestToggle);
				if (_singleInstance)
					registration.SingleInstance();
				builder.RegisterType<MyAspect>().As<IAspect>().SingleInstance();
			}
		}
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddModule(new extraComponentsInTest(_singleInstance));
		}
		
		[TestCase(true, ExpectedResult = "on")]
		[TestCase(false, ExpectedResult = "off")]
		public string ShouldReturnCorrectType(bool toggleValue)
		{
			ToggleManager.Set(Toggles.TestToggle, toggleValue);

			return MyService.Value;
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldRegisterProxyWithCorrectScope(bool toggleValue)
		{
			ToggleManager.Set(Toggles.TestToggle, toggleValue);
		
			var svc1 = LifetimeScope.Resolve<IMyService>();
			var svc2 = LifetimeScope.Resolve<IMyService>();

			if (_singleInstance)
			{
				svc1.Should().Be.SameInstanceAs(svc2);
			}
			else
			{
				svc1.Should().Not.Be.SameInstanceAs(svc2);
			}
		}
		
		[TestCase(true)]
		[TestCase(false)]
		public void ShouldRegisterServiceWithCorrectScope(bool toggleValue)
		{
			ToggleManager.Set(Toggles.TestToggle, toggleValue);

			LifetimeScope.Resolve<IMyService>().Counter();
			LifetimeScope.Resolve<IMyService>().Counter();
			var result = LifetimeScope.Resolve<IMyService>().Counter();

			result.Should().Be.EqualTo(_singleInstance ? 3 : 1);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void OnlyOneMyServiceShouldBeRegistered(bool toggleValue)
		{
			ToggleManager.Set(Toggles.TestToggle, toggleValue);

			LifetimeScope.Resolve<IEnumerable<IMyService>>().Count()
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldBeAbleToChangeReturnedTypeOnTheFly()
		{
			ToggleManager.Disable(Toggles.TestToggle);
			MyService.Value.Should().Be.EqualTo("off");
			
			ToggleManager.Enable(Toggles.TestToggle);
			MyService.Value.Should().Be.EqualTo("on");
		}

		public static bool aspectWasCalled;
		[TestCase(true)]
		[TestCase(false)]
		public void ShouldHandleOurAspects(bool toggleValue)
		{
			ToggleManager.Set(Toggles.TestToggle, toggleValue);
			aspectWasCalled = false;
			
			MyService.AspectMethod();
			
			aspectWasCalled.Should().Be.True();
		}

		public class MyServiceOn : IMyService
		{
			private int _counter;
			public int Counter() => ++_counter;
			public string Value { get; } = "on";
			[MyAspect]
			public virtual void AspectMethod()
			{
			}
		}

		public class MyServiceOff : IMyService
		{
			private int _counter;
			public int Counter() => ++_counter;

			[MyAspect]
			public virtual void AspectMethod()
			{
			}
			public string Value { get; } = "off";
		}
		
		public interface IMyService
		{
			string Value { get; }
			int Counter();
			void AspectMethod();
		}

		public class MyAspectAttribute : AspectAttribute
		{
			public MyAspectAttribute() : base(typeof(MyAspect))
			{
			}
		}

		private class MyAspect : IAspect
		{
			public void OnBeforeInvocation(IInvocationInfo invocation)
			{
				aspectWasCalled = true;
			}

			public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
			{
			}
		}
	}
}