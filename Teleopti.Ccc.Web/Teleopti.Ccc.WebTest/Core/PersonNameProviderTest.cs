using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core
{
	[TestFixture]
	class PersonNameProviderTest
	{
		private ISettingsPersisterAndProvider<NameFormatSettings> _nameFormatSettingsPersisterAndProvider;

		[SetUp]
		public void Setup()
		{
			_nameFormatSettingsPersisterAndProvider = MockRepository.GenerateMock<ISettingsPersisterAndProvider<NameFormatSettings>>();
		}

		[Test]
		public void ShouldBuildDefalultName()
		{
			var target = new PersonNameProvider(_nameFormatSettingsPersisterAndProvider);
			var name =  new Name("Agent", "Name") ;
			string expectName = name.FirstName + " " + name.LastName;

			var result = target.BuildNameFromSetting(name);

			result.Should().Be.EqualTo(expectName);
		}		
		
		[Test]
		public void ShouldBuildNameBySettingId0()
		{
			_nameFormatSettingsPersisterAndProvider.Stub(x => x.Get()).Return(new NameFormatSettings() {NameFormatId = 0});
			var target = new PersonNameProvider(_nameFormatSettingsPersisterAndProvider);
			var name = new Name("Agent", "Name");
			string firstNameLastName = name.FirstName + " " + name.LastName;

			var result = target.BuildNameFromSetting(name);

			result.Should().Be.EqualTo(firstNameLastName);
		}		
		
		[Test]
		public void ShouldBuildNameBySettingId1()
		{
			_nameFormatSettingsPersisterAndProvider.Stub(x => x.Get()).Return(new NameFormatSettings() {NameFormatId = 1});
			var target = new PersonNameProvider(_nameFormatSettingsPersisterAndProvider);
			var name = new Name("Agent", "Name");
			string lastNameFirstName = name.LastName + " " + name.FirstName;

			var result = target.BuildNameFromSetting(name);

			result.Should().Be.EqualTo(lastNameFirstName);
		}

		[Test]
		public void ShouldBuildNamesBaseOnMySettingSoDontHaveToHitPersonRepositoryEveryTime()
		{
			var mySettting = new NameFormatSettings() {NameFormatId = 0};
			var target = new PersonNameProvider(_nameFormatSettingsPersisterAndProvider);
			var myName = new Name("Agent", "Name");

			var result = target.BuildNameFromSetting(myName.FirstName, myName.LastName, mySettting);
			result.Should().Be.EqualTo(myName.FirstName + " " + myName.LastName);
		}
	}
}
