﻿using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	[TestFixture]
	class UseOnAndNotOnToggleTest
	{

		[Test]
		public void ShouldBeEnabledIfUseOnToggleIsTrueAndUseNotOnToggleIsFalse()
		{
			var iocConfig = MockRepository.GenerateMock<IIocConfiguration>();
			iocConfig.Expect(m => m.Toggle(Toggles.TestToggle)).Return(true);
			iocConfig.Expect(m => m.Toggle(Toggles.TestToggle2)).Return(false);

			Assert.That(typeof(targetWithMultiAttribOnNotOn).TypeEnabledByToggle(iocConfig), Is.True);
		}

		[Test]
		public void ShouldNotBeEnabledIfUseOnToggleIsFalseAndUseNotOnToggleIsFalse()
		{
			var iocConfig = MockRepository.GenerateMock<IIocConfiguration>();
			iocConfig.Expect(m => m.Toggle(Toggles.TestToggle)).Return(false);
			iocConfig.Expect(m => m.Toggle(Toggles.TestToggle2)).Return(false);

			Assert.That(typeof(targetWithMultiAttribOnNotOn).TypeEnabledByToggle(iocConfig), Is.False);
		}

		[Test]
		public void ShouldNotBeEnabledIfUseOnToggleIsTrueAndUseNotOnToggleIsTrue()
		{
			var iocConfig = MockRepository.GenerateMock<IIocConfiguration>();
			iocConfig.Expect(m => m.Toggle(Toggles.TestToggle)).Return(true);
			iocConfig.Expect(m => m.Toggle(Toggles.TestToggle2)).Return(true);

			Assert.That(typeof(targetWithMultiAttribOnNotOn).TypeEnabledByToggle(iocConfig), Is.False);
		}

		[Test]
		public void ShouldNotBeEnabledIfUseOnToggleIsFalseAndUseNotOnToggleIsTrue()
		{
			var iocConfig = MockRepository.GenerateMock<IIocConfiguration>();
			iocConfig.Expect(m => m.Toggle(Toggles.TestToggle)).Return(false);
			iocConfig.Expect(m => m.Toggle(Toggles.TestToggle2)).Return(true);

			Assert.That(typeof(targetWithMultiAttribOnNotOn).TypeEnabledByToggle(iocConfig), Is.False);
		}

		[Test]
		public void ShouldNotBeEnabledIfUseOnToggleIsTrueAndUseNotOnToggleIsFalseAndTrue()
		{
			var iocConfig = MockRepository.GenerateMock<IIocConfiguration>();
			iocConfig.Expect(m => m.Toggle(Toggles.TestToggle)).Return(true);
			iocConfig.Expect(m => m.Toggle(Toggles.TestToggle2)).Return(false);
			iocConfig.Expect(m => m.Toggle(Toggles.TestToggle3)).Return(true);

			Assert.That(typeof(targetWithMultiAttribOnNotOn2).TypeEnabledByToggle(iocConfig), Is.False);
		}

		[EnabledBy(Toggles.TestToggle), DisabledBy(Toggles.TestToggle2)]
		private class targetWithMultiAttribOnNotOn
		{
		}

		[EnabledBy(Toggles.TestToggle), DisabledBy(Toggles.TestToggle2, Toggles.TestToggle3)]
		private class targetWithMultiAttribOnNotOn2
		{
		}
	}
}
