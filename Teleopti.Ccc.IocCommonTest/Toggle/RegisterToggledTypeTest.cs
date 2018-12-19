using System;
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
	public class RegisterToggledTypeTest : IExtendSystem
	{
		public FakeToggleManager ToggleManager;
		public IMyService MyService;
		public ILifetimeScope LifetimeScope;

		private class extraComponentsInTest : Module
		{
			protected override void Load(ContainerBuilder builder)
			{
				builder.RegisterToggledTypeTest<MyServiceOn, MyServiceOff, IMyService>(Toggles.TestToggle);
				builder.RegisterType<MyAspect>().As<IAspect>().SingleInstance();
			}
		}
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddModule(new extraComponentsInTest());
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
		public void ShouldRegisterProxyAsSingleton(bool toggleValue)
		{
			ToggleManager.Set(Toggles.TestToggle, toggleValue);
		
			LifetimeScope.Resolve<IMyService>()
				.Should().Be.SameInstanceAs(LifetimeScope.Resolve<IMyService>());
		}
		
		[TestCase(true)]
		[TestCase(false)]
		public void ShouldRegisterServiceAsSingleton(bool toggleValue)
		{
			ToggleManager.Set(Toggles.TestToggle, toggleValue);
		
			(LifetimeScope.Resolve<IMyService>().Counter + 1)
				.Should().Be.EqualTo(LifetimeScope.Resolve<IMyService>().Counter);
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
			public int Counter => _counter++;
			public string Value { get; } = "on";
			[MyAspect]
			public virtual void AspectMethod()
			{
			}
		}

		public class MyServiceOff : IMyService
		{
			private int _counter;
			public int Counter => _counter++;
			[MyAspect]
			public virtual void AspectMethod()
			{
			}
			public string Value { get; } = "off";
		}
		
		public interface IMyService
		{
			string Value { get; }
			int Counter { get; }
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