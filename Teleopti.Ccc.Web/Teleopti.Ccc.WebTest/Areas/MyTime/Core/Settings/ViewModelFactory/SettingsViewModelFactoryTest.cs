using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Settings.ViewModelFactory
{
	[TestFixture]
	public class SettingsViewModelFactoryTest
	{
		private SettingsViewModelFactory _target;
		private IMappingEngine _mapper;
		private ILoggedOnUser _loggedOnUser;

		[SetUp]
		public void Setup()
		{
			var person = PersonFactory.CreatePersonWithGuid("", "");
			_target = new SettingsViewModelFactory();
			Mapper.Initialize(c => c.AddProfile(new SettingsMappingProfile()));
			_mapper = Mapper.Engine; 
			_loggedOnUser = MockRepository.GenerateStrictMock<ILoggedOnUser>();
			_loggedOnUser.Expect(obj => obj.CurrentUser()).Return(person);
		}

		
		[Test]
		public void ShouldLoadNameFormats()
		{
			var result = _target.CreateViewModel(_mapper, _loggedOnUser);
			Assert.That(result.NameFormats.First().text, Is.EqualTo(Resources.AgentNameFormatFirstNameLastName));
			Assert.That(result.NameFormats.Last().text, Is.EqualTo(Resources.AgentNameFormatLastNameFirstName));
			Assert.That(result.NameFormats.First().id, Is.EqualTo(0));
			Assert.That(result.NameFormats.Last().id, Is.EqualTo(1));
		}

		[Test]
		public void ShouldLoadDefaultNameFormat()
		{			
			var result = _target.CreateViewModel(_mapper, _loggedOnUser);
			Assert.That(result.ChosenNameFormat.text, Is.EqualTo(Resources.AgentNameFormatFirstNameLastName));
			Assert.That(result.ChosenNameFormat.id, Is.EqualTo(0));
		}
	}
}
