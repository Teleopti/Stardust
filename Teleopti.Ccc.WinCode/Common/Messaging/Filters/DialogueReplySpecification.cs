﻿using System;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.WinCode.Common.Messaging.Filters
{
    public class DialogueReplySpecification : Specification<IFollowUpMessageDialogueViewModel>
    {
        public string Reply { get; private set;}

        public DialogueReplySpecification(string reply)
        {
            Reply = reply;
        }
        public override bool IsSatisfiedBy(IFollowUpMessageDialogueViewModel obj)
        {
            return string.Equals(Reply, obj.GetReply(new NoFormatting()));
        }
    }
}
