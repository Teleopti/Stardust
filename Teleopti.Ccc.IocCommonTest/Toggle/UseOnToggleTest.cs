using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	[TestFixture]
	public class UseOnToggleTest
	{
		[Test]
		public void ShouldBeEnabledIfMultipleToggleIsEnabledByAttribute()
		{
			var iocConfig = MockRepository.GenerateMock<IocConfiguration>();
			iocConfig.Expect(m => m.IsToggleEnabled(Toggles.TestToggle)).Return(true);
			iocConfig.Expect(m => m.IsToggleEnabled(Toggles.TestToggle2)).Return(true);

			Assert.That(typeof(targetWithMultiAttrib).EnabledByToggle(iocConfig), Is.True);
		}

		[Test]
		public void ShouldBeEnabledIfToggleIsEnabledByAttribute()
		{
			var iocConfig = MockRepository.GenerateMock<IocConfiguration>();
			iocConfig.Expect(m => m.IsToggleEnabled(Toggles.TestToggle)).Return(true);

			Assert.That(typeof(targetWithAttrib).EnabledByToggle(iocConfig), Is.True);
		}

		[Test]
		public void ShouldBeEnabledIfNoAttribute()
		{
			var iocConfig = MockRepository.GenerateMock<IocConfiguration>();
			iocConfig.Expect(m => m.IsToggleEnabled(Toggles.TestToggle)).Return(false).Repeat.Any();

			Assert.That(typeof(targetWithoutAttrib).EnabledByToggle(iocConfig), Is.True);
		}

		[Test]
		public void ShouldBeNotEnabledIfMultipleTogglesAreBothEnabledAndDisabledByAttribute()
		{
			var iocConfig = MockRepository.GenerateMock<IocConfiguration>();
			iocConfig.Expect(m => m.IsToggleEnabled(Toggles.TestToggle)).Return(true);
			iocConfig.Expect(m => m.IsToggleEnabled(Toggles.TestToggle2)).Return(false);

			Assert.That(typeof(targetWithMultiAttrib).EnabledByToggle(iocConfig), Is.False);
		}

		[Test]
		public void ShouldNotBeEnabledIfToggleIsFalseByAttribute()
		{
			var iocConfig = MockRepository.GenerateMock<IocConfiguration>();
			iocConfig.Expect(m => m.IsToggleEnabled(Toggles.TestToggle)).Return(false);

			Assert.That(typeof(targetWithAttrib).EnabledByToggle(iocConfig), Is.False);
		}

		[EnabledBy(Toggles.TestToggle, Toggles.TestToggle2)]
		private class targetWithMultiAttrib
		{
		}

		[EnabledBy(Toggles.TestToggle)]
		private class targetWithAttrib
		{
		}

		private class targetWithoutAttrib
		{
		}

	}
}