using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.WinCode.Common.Messaging.Filters
{
    public class DialogueHasMoreMessagesThan: Specification<IFollowUpMessageDialogueViewModel>
    {
        private int _numberOfConversations;

        public DialogueHasMoreMessagesThan(int numberOfConversations)
        {
            _numberOfConversations = numberOfConversations;
        }
        public override bool IsSatisfiedBy(IFollowUpMessageDialogueViewModel obj)
        {
            return obj.Messages.Count > _numberOfConversations;
        }
    }
}
