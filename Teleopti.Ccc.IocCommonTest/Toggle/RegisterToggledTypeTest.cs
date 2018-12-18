using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	public class RegisterToggledTypeTest
	{
		[TestCase(true, ExpectedResult = "on")]
		[TestCase(false, ExpectedResult = "off")]
		public string ShouldReturnCorrectType(bool toggleValue)
		{
			var toggleManager = new FakeToggleManager();
			toggleManager.Set(Toggles.TestToggle, toggleValue);
			var builder = new ContainerBuilder();
			builder.RegisterToggledTypeTest<MyServiceOn, MyServiceOff, IMyService>(toggleManager, Toggles.TestToggle);
			var container = builder.Build();

			return container.Resolve<IMyService>().Value;
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldBeRegisteredAsSingleton(bool toggleValue)
		{
			var toggleManager = new FakeToggleManager();
			toggleManager.Set(Toggles.TestToggle, toggleValue);
			var builder = new ContainerBuilder();
			builder.RegisterToggledTypeTest<MyServiceOn, MyServiceOff, IMyService>(toggleManager, Toggles.TestToggle);
			var container = builder.Build();

			container.Resolve<IMyService>()
				.Should().Be.SameInstanceAs(container.Resolve<IMyService>());
		}

		[Test]
		public void ShouldBeAbleToChangeReturnedTypeOnTheFly()
		{
			var toggleManager = new FakeToggleManager();
			toggleManager.Disable(Toggles.TestToggle);
			var builder = new ContainerBuilder();
			builder.RegisterToggledTypeTest<MyServiceOn, MyServiceOff, IMyService>(toggleManager, Toggles.TestToggle);
			var container = builder.Build();

			var component = container.Resolve<IMyService>();
			
			component.Value.Should().Be.EqualTo("off");
			toggleManager.Enable(Toggles.TestToggle);
			component.Value.Should().Be.EqualTo("on");
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