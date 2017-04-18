using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Messaging
{
   
    public interface IFollowUpMessageDialogueViewModel
    {
        IPerson Receiver { get; }
		ObservableCollection<DialogueMessageViewModel> Messages { get; }
        string ReplyText { get; set; }
        bool IsReplied { get; }
    	string GetReply(ITextFormatter formatter);
        bool AllowDialogueReply { get; set; }
    }
}
