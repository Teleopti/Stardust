using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
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
			using (var target = new UserTokenController(settingsRepository))
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
			using (var target = new UserTokenController(settingsRepository))
			{
				target.Post("asdfasdf");
				target.Post("asdfasdf");
			}
			var result = settingsRepository.FindValueByKey(UserDevices.Key, new UserDevices());
			result.TokenList.Single().Should().Be.EqualTo("asdfasdf");
		}

		[Test]
		public void ShouldGetUserTokens()
		{
			var settingsRepository = new FakePersonalSettingDataRepository();
			var userDevices = new UserDevices();
			var token = "asdfkjlkjasdf-asdfkjlkjasdf-asdfkjlkjasdf";
			userDevices.AddToken(token);
			settingsRepository.PersistSettingValue(UserDevices.Key, userDevices);
			using (var target = new UserTokenController(settingsRepository))
			{
				var result = (OkNegotiatedContentResult<string[]>)target.Get();
				result.Content[0].Should().Be.EqualTo(token);
			}
		}
	}
}