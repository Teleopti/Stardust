using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
    [TestFixture]
    public class MessageControllerTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
        public void ShouldReturnMessagePartialView()
        {
            var target = new MessageController(MockRepository.GenerateMock<IMessageViewModelFactory>());

            var result = target.Index();

            result.ViewName.Should().Be.EqualTo("MessagePartial");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
        public void ShouldReturnViewModelForMessages()
        {
            var viewModelFactory = MockRepository.GenerateMock<IMessageViewModelFactory>();
            var target = new MessageController(viewModelFactory);
            var model = new MessageViewModel[] { };
        	var paging = new Paging();

            viewModelFactory.Stub(x => x.CreatePageViewModel(paging)).Return(model);

            var result = target.Messages(paging);

            result.Data.Should().Be.SameInstanceAs(model);
        }
    }
}
