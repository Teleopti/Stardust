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
		public void ShouldBeEnabledIfToggleIsEnabledByAttribute()
		{
			var iocConfig = MockRepository.GenerateMock<IIocConfiguration>();
			iocConfig.Expect(m => m.Toggle(Toggles.TestToggle)).Return(true);

			Assert.That(typeof(targetWithAttrib).TypeEnabledByToggle(iocConfig), Is.True);
		}

		[Test]
		public void ShouldBeEnabledIfNoAttribute()
		{
			var iocConfig = MockRepository.GenerateMock<IIocConfiguration>();
			iocConfig.Expect(m => m.Toggle(Toggles.TestToggle)).Return(false).Repeat.Any();

			Assert.That(typeof(targetWithoutAttrib).TypeEnabledByToggle(iocConfig), Is.True);
		}

		[Test]
		public void ShouldNotBeEnabledIfToggleIsFalseByAttribute()
		{
			var iocConfig = MockRepository.GenerateMock<IIocConfiguration>();
			iocConfig.Expect(m => m.Toggle(Toggles.TestToggle)).Return(false);

			Assert.That(typeof(targetWithAttrib).TypeEnabledByToggle(iocConfig), Is.False);
		}

		[UseOnToggle(Toggles.TestToggle)]
		private class targetWithAttrib
		{
		}

		private class targetWithoutAttrib
		{
		}

	}
}