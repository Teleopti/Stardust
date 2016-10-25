using System;
using System.Collections.Generic;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Message.ViewModelFactory
{
    public class MessageViewModelFactory : IMessageViewModelFactory
    {
        private readonly IPushMessageProvider _messageProvider;
        private readonly IMappingEngine _mapper;

        public MessageViewModelFactory(IPushMessageProvider messageProvider, IMappingEngine mapper)
        {
            _messageProvider = messageProvider;
            _mapper = mapper;
        }

        public IList<MessageViewModel> CreatePageViewModel(Paging paging)
        {
            var messages = _messageProvider.GetMessages(paging);

            return _mapper.Map<IList<IPushMessageDialogue>, IList<MessageViewModel>>(messages);
        }

    	public MessagesInformationViewModel CreateMessagesInformationViewModel(Guid messageId)
    	{
			return new MessagesInformationViewModel
			       	{
						UnreadMessagesCount = _messageProvider.UnreadMessageCount,
						MessageItem = _mapper.Map<IPushMessageDialogue, MessageViewModel>(_messageProvider.GetMessage(messageId))
			       	};
    	}
    }
}