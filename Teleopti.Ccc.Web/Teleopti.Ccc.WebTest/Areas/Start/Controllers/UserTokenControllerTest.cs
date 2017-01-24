using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Start.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class UserTokenControllerTest
	{
		[Test]
		public void ShouldStoreUserToken()
		{
			var settingsRepository = new FakePersonalSettingDataRepository();
			var person = PersonFactory.CreatePerson();
			var loggedOnUser = new FakeLoggedOnUser(person);
			using (var target = new UserTokenController(settingsRepository,loggedOnUser))
			{
				target.Post("asdfasdf");
			}
			var result = settingsRepository.FindValueByKey(UserDevices.Key, new UserDevices());
			result.TokenList.Single().Should().Be.EqualTo("asdfasdf");
		}

		[Test]
		public void ShouldNotStoreDuplicateUserToken()
		{
			var settingsRepository = new FakePersonalSettingDataRepository();
			var person = PersonFactory.CreatePerson();
			var loggedOnUser = new FakeLoggedOnUser(person);
			using (var target = new UserTokenController(settingsRepository,loggedOnUser))
			{
				target.Post("asdfasdf");
				target.Post("asdfasdf");
			}
			var result = settingsRepository.FindValueByKey(UserDevices.Key, new UserDevices());
			result.TokenList.Single().Should().Be.EqualTo("asdfasdf");
		}
	}
}