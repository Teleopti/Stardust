using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
    [TestFixture]
    public class MessageControllerTest
    {
        [Test]
        public void ShouldReturnMessagePartialView()
        {
            var target = new MessageController(MockRepository.GenerateMock<IMessageViewModelFactory>());

            var result = target.Index();

            result.ViewName.Should().Be.EqualTo("MessagePartial");
        }

        [Test]
        public void ShouldReturnViewModelForMessages()
        {
            var viewModelFactory = MockRepository.GenerateMock<IMessageViewModelFactory>();
            var target = new MessageController(viewModelFactory);
            var model = new MessageViewModel[] { };

            viewModelFactory.Stub(x => x.CreatePageViewModel()).Return(model);

            var result = target.Messages();

            result.Data.Should().Be.SameInstanceAs(model);
        }
    }
}
