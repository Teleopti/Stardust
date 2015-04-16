using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Asm;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class MessageBrokerControllerTest
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