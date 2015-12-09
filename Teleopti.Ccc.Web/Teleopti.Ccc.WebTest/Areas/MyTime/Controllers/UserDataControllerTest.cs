using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker;
using Teleopti.Ccc.WebTest.TestHelper;

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
			userDataFactory.Expect(fac => fac.CreateViewModel()).IgnoreArguments().Return(expected);

			using (var controller = new UserDataController(userDataFactory))
			{
				var model = controller.FetchUserData();
				model.Result<UserData>().Should().Be.SameInstanceAs(expected);
			}
		}
	}
}