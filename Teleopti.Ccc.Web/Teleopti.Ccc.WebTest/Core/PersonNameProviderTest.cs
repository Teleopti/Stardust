using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
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
			var person = new Person { Name = new Name("a", "person") };
			string name = person.Name.FirstName + " " + person.Name.LastName;

			var result = target.BuildNameFromSetting(person);

			result.Should().Be.EqualTo(name);
		}		
		
		[Test]
		public void ShouldBuildNameBySettingId0()
		{
			_nameFormatSettingsPersisterAndProvider.Stub(x => x.Get()).Return(new NameFormatSettings() {NameFormatId = 0});
			var target = new PersonNameProvider(_nameFormatSettingsPersisterAndProvider);
			var person = new Person { Name = new Name("a", "person") };
			string firstNameLastName = person.Name.FirstName + " " + person.Name.LastName;

			var result = target.BuildNameFromSetting(person);

			result.Should().Be.EqualTo(firstNameLastName);
		}		
		
		[Test]
		public void ShouldBuildNameBySettingId1()
		{
			_nameFormatSettingsPersisterAndProvider.Stub(x => x.Get()).Return(new NameFormatSettings() {NameFormatId = 1});
			var target = new PersonNameProvider(_nameFormatSettingsPersisterAndProvider);
			var person = new Person { Name = new Name("a", "person") };
			string lastNameFirstName = person.Name.LastName + " " + person.Name.FirstName;

			var result = target.BuildNameFromSetting(person);

			result.Should().Be.EqualTo(lastNameFirstName);
		}
	}
}
