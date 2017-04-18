using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Messaging
{
    public interface IFollowUpPushMessageViewModel
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IList<IObservable<FollowUpPushMessageViewModel>> Observables { get; }
        ObservableCollection<IFollowUpMessageDialogueViewModel> Dialogues { get; }
    	string Title { get; }
    	string Message { get; }
        IList<ReplyOptionViewModel> ReplyOptions { get; }
        CommandModel Delete { get; }
        CommandModel LoadDialogues { get; }
    }
}
