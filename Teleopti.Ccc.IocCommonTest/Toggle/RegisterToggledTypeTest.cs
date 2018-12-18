using System;
using Autofac;
using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	public class RegisterToggledTypeTest
	{
		[TestCase(true, ExpectedResult = typeof(MyServiceOn))]
		[TestCase(false, ExpectedResult = typeof(MyServiceOff))]
		public Type ShouldReturnCorrectType(bool toggleValue)
		{
			var toggleManager = new FakeToggleManager();
			toggleManager.Set(Toggles.TestToggle, toggleValue);
			var builder = new ContainerBuilder();
			builder.RegisterToggledTypeTest<MyServiceOn, MyServiceOff, IMyService>(toggleManager, Toggles.TestToggle);
			var container = builder.Build();

			return container.Resolve<IMyService>().GetType();
		}

		public class MyServiceOn : IMyService
		{
		}

		public class MyServiceOff : IMyService
		{
		}
		
		public interface IMyService
		{
		}
	}
}