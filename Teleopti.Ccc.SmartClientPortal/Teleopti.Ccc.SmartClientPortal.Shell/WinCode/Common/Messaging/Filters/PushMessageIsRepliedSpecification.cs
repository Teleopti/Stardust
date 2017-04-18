using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Messaging.Filters
{
    /// <summary>
    /// Satisfied if all dialogues are replied
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-05-29
    /// </remarks>
    public class PushMessageIsRepliedSpecification : Specification<IFollowUpPushMessageViewModel>
    {
        public override bool IsSatisfiedBy(IFollowUpPushMessageViewModel obj)
        {
            foreach (IFollowUpMessageDialogueViewModel dialogue in obj.Dialogues)
            {
                if (!dialogue.IsReplied) return false;
            }
            return true;
        }
    }
}
