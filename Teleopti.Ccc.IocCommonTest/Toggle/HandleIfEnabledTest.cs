using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	[TestFixture]
	public class HandleIfEnabledTest
	{
		[Test]
		public void ShouldBeEnabledIfToggleIsEnabledByAttribute()
		{
			var iocConfig = MockRepository.GenerateMock<IIocConfiguration>();
			iocConfig.Expect(m => m.Toggle(Toggles.TestToggle)).Return(true);

			Assert.That(typeof(targetWithAttrib).FeatureIsEnabled(iocConfig), Is.True);
		}

		[Test]
		public void ShouldBeEnabledIfNoAttribute()
		{
			var iocConfig = MockRepository.GenerateMock<IIocConfiguration>();
			iocConfig.Expect(m => m.Toggle(Toggles.TestToggle)).Return(false).Repeat.Any();

			Assert.That(typeof(targetWithoutAttrib).FeatureIsEnabled(iocConfig), Is.True);
		}

		[Test]
		public void ShouldNotBeEnabledIfToggleIsFalseByAttribute()
		{
			var iocConfig = MockRepository.GenerateMock<IIocConfiguration>();
			iocConfig.Expect(m => m.Toggle(Toggles.TestToggle)).Return(false);

			Assert.That(typeof(targetWithAttrib).FeatureIsEnabled(iocConfig), Is.False);
		}

		[OnlyHandleWhenEnabled(Toggles.TestToggle)]
		private class targetWithAttrib
		{
			 
		}


		private class targetWithoutAttrib
		{

		}
	}
}