using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Messaging.Filters
{
    public class DialogueIsRepliedSpecification:Specification<IFollowUpMessageDialogueViewModel>
    {
        public override bool IsSatisfiedBy(IFollowUpMessageDialogueViewModel obj)
        {
            return !obj.IsReplied;
        }
    }
}
