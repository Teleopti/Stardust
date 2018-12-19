using Autofac;
using NUnit.Framework;
using SharpTestsEx;
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
				builder.RegisterToggledTypeTest<MyServiceOn, MyServiceOff, IMyService>(null, Toggles.TestToggle);
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
		public void ShouldBeRegisteredAsSingleton(bool toggleValue)
		{
			ToggleManager.Set(Toggles.TestToggle, toggleValue);
		
			LifetimeScope.Resolve<IMyService>()
				.Should().Be.SameInstanceAs(LifetimeScope.Resolve<IMyService>());
		}

		[Test]
		public void ShouldBeAbleToChangeReturnedTypeOnTheFly()
		{
			ToggleManager.Disable(Toggles.TestToggle);
			MyService.Value.Should().Be.EqualTo("off");
			
			ToggleManager.Enable(Toggles.TestToggle);
			MyService.Value.Should().Be.EqualTo("on");
		}

		public class MyServiceOn : IMyService
		{
			public string Value { get; } = "on";
		}

		public class MyServiceOff : IMyService
		{
			public string Value { get; } = "off";
		}
		
		public interface IMyService
		{
			string Value { get; }
		}
	}
}