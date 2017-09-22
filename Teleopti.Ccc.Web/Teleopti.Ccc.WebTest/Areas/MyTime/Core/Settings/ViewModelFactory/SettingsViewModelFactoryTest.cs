using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.ViewModelFactory;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Settings.ViewModelFactory
{
	[TestFixture]
	public class SettingsViewModelFactoryTest
	{
		private SettingsViewModelFactory _target;
		private ILoggedOnUser _loggedOnUser;

		[SetUp]
		public void Setup()
		{
			_loggedOnUser = MockRepository.GenerateStrictMock<ILoggedOnUser>();
			_loggedOnUser.Expect(obj => obj.CurrentUser()).Return(PersonFactory.CreatePersonWithGuid("", ""));
		}


		[Test]
		public void ShouldLoadNameFormats()
		{
			var nameFormatPersisterAndProvider = MockRepository.GenerateStrictMock<ISettingsPersisterAndProvider<NameFormatSettings>>();
			nameFormatPersisterAndProvider.Expect(obj => obj.Get()).Return(new NameFormatSettings() { NameFormatId = 0 });
			_target = new SettingsViewModelFactory(new SettingsMapper(), _loggedOnUser, nameFormatPersisterAndProvider, new FakeNoPermissionProvider());

			var result = _target.CreateViewModel();
			Assert.That(result.NameFormats.First().text, Is.EqualTo(Resources.AgentNameFormatFirstNameLastName));
			Assert.That(result.NameFormats.Last().text, Is.EqualTo(Resources.AgentNameFormatLastNameFirstName));
			Assert.That(result.NameFormats.First().id, Is.EqualTo(0));
			Assert.That(result.NameFormats.Last().id, Is.EqualTo(1));
		}

		[Test]
		public void ShouldLoadDefaultNameFormat()
		{
			var nameFormatPersisterAndProvider = MockRepository.GenerateStrictMock<ISettingsPersisterAndProvider<NameFormatSettings>>();
			nameFormatPersisterAndProvider.Expect(obj => obj.Get()).Return(new NameFormatSettings() { NameFormatId = 0 });
			_target = new SettingsViewModelFactory(new SettingsMapper(), _loggedOnUser, nameFormatPersisterAndProvider, new FakePermissionProvider());

			var result = _target.CreateViewModel();
			Assert.That(result.ChosenNameFormat.text, Is.EqualTo(Resources.AgentNameFormatFirstNameLastName));
			Assert.That(result.ChosenNameFormat.id, Is.EqualTo(0));
		}

		[Test]
		public void ShouldLoadChosenNameFormat()
		{
			var nameFormatPersisterAndProvider = MockRepository.GenerateStrictMock<ISettingsPersisterAndProvider<NameFormatSettings>>();
			nameFormatPersisterAndProvider.Expect(obj => obj.Get()).Return(new NameFormatSettings() { NameFormatId = 1 });
			_target = new SettingsViewModelFactory(new SettingsMapper(), _loggedOnUser, nameFormatPersisterAndProvider, new Global.FakePermissionProvider());

			var result = _target.CreateViewModel();
			Assert.That(result.ChosenNameFormat.text, Is.EqualTo(Resources.AgentNameFormatLastNameFirstName));
			Assert.That(result.ChosenNameFormat.id, Is.EqualTo(1));
		}

		[Test]
		public void ShouldGetPermissionForViewingQRCode()
		{
			var permissionProvider = new Global.FakePermissionProvider();
			permissionProvider.Enable();
			var nameFormatPersisterAndProvider = MockRepository.GenerateStrictMock<ISettingsPersisterAndProvider<NameFormatSettings>>();
			nameFormatPersisterAndProvider.Expect(obj => obj.Get()).Return(new NameFormatSettings() { NameFormatId = 1 });
			_target = new SettingsViewModelFactory(new SettingsMapper(), _loggedOnUser, nameFormatPersisterAndProvider, permissionProvider);

			var result = _target.CreateViewModel();
			Assert.That(result.HasPermissionToViewQRCode, Is.EqualTo(false));
		}
	}
}
