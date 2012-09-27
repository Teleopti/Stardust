using System.Collections.Generic;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Message.ViewModelFactory
{
    [TestFixture]
    public class MessageViewModelFactoryTest
    {
        [Test]
        public void ShouldReturnZeroViewModelsFromPushMessages()
        {
            var messageProvider = MockRepository.GenerateMock<IPushMessageProvider>();
            var mapper = MockRepository.GenerateMock<IMappingEngine>();

            var target = new MessageViewModelFactory(messageProvider, mapper);

            IList<IPushMessageDialogue> domainMessages = new List<IPushMessageDialogue>();
            messageProvider.Stub(x => x.GetMessages()).Return(domainMessages);
            mapper.Stub(x => x.Map<IList<IPushMessageDialogue>, IList<MessageViewModel>>(domainMessages)).Return(
                new List<MessageViewModel>());

            IList<MessageViewModel> result = target.CreatePageViewModel();

            result.Should().Not.Be.Null();
            result.Count.Should().Be.EqualTo(0);
        }
    }
}
