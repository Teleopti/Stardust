using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Messaging
{
   
    public interface IFollowUpMessageDialogueViewModel
    {
        IPerson Receiver { get; }
        ObservableCollection<IDialogueMessage> Messages { get; }
        string ReplyText { get; set; }
        bool IsReplied { get; }
    	string GetReply(ITextFormatter formatter);
        bool AllowDialogueReply { get; set; }
    }
}
