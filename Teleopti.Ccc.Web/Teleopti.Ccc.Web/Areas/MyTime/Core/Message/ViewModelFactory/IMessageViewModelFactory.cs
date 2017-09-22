using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Message.ViewModelFactory
{
    public interface IMessageViewModelFactory
    {
        IList<MessageViewModel> CreatePageViewModel(Paging paging);
    	MessagesInformationViewModel CreateMessagesInformationViewModel(Guid messageId);
    }
}