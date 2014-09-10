using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class UserDataControllerTest
	{
		[Test]
		public void ShouldRetrieveMessageBrokerModel()
		{
			var expected = new UserData();
			var userDataFactory = MockRepository.GenerateMock<IUserDataFactory>();
			userDataFactory.Expect(fac => fac.CreateViewModel()).Return(expected);

			using (var controller = new UserDataController(userDataFactory))
			{
				var model = controller.FetchUserData();
				model.Data.Should().Be.SameInstanceAs(expected);
			}
		}
	}
}