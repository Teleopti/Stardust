using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Message.ViewModelFactory
{
    public interface IMessageViewModelFactory
    {
        IList<MessageViewModel> CreatePageViewModel();
    }
}