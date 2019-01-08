using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	[TestFixture]
	public class UseNotOnToggleTest
	{

		[Test]
		public void ShouldBeEnabledIfToggleIsDisabled()
		{
			var iocConfig = MockRepository.GenerateMock<IocConfiguration>();
			iocConfig.Expect(m => m.IsToggleEnabled(Toggles.TestToggle)).Return(false);

			Assert.That(typeof(targetWithAttribNotOn).EnabledByToggle(iocConfig), Is.True);
		}


		[Test]
		public void ShouldBeEnabledIfMultipleToggleAreDisabled()
		{
			var iocConfig = MockRepository.GenerateMock<IocConfiguration>();
			iocConfig.Expect(m => m.IsToggleEnabled(Toggles.TestToggle)).Return(false);
			iocConfig.Expect(m => m.IsToggleEnabled(Toggles.TestToggle2)).Return(false);

			Assert.That(typeof(targetWithMultiAttribNotOn).EnabledByToggle(iocConfig), Is.True);
		}


		[Test]
		public void ShouldNotBeEnabledIfAtLeastOneToggleIsEnabled()
		{
			var iocConfig = MockRepository.GenerateMock<IocConfiguration>();
			iocConfig.Expect(m => m.IsToggleEnabled(Toggles.TestToggle)).Return(true);
			iocConfig.Expect(m => m.IsToggleEnabled(Toggles.TestToggle2)).Return(false);

			Assert.That(typeof(targetWithMultiAttribNotOn).EnabledByToggle(iocConfig), Is.False);
		}
		

		[DisabledBy(Toggles.TestToggle)]
		private class targetWithAttribNotOn
		{
		}

		[DisabledBy(Toggles.TestToggle, Toggles.TestToggle2)]
		private class targetWithMultiAttribNotOn
		{
		}
	}
}
