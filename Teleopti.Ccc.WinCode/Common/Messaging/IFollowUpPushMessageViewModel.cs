using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.WinCode.Common.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Messaging
{
    public interface IFollowUpPushMessageViewModel
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IList<IObservable<FollowUpPushMessageViewModel>> Observables { get; }
        ObservableCollection<IFollowUpMessageDialogueViewModel> Dialogues { get; }
    	string GetTitle(ITextFormatter formatter);
		string GetMessage(ITextFormatter formatter);
        IList<ReplyOptionViewModel> ReplyOptions { get; }
        CommandModel Delete { get; }
        CommandModel LoadDialogues { get; }

    }
}
