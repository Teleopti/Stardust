using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.Messaging
{
    public class CreatePushMessageDialoguesService : ICreatePushMessageDialoguesService, ISendPushMessageReceipt
    {
        public ISendPushMessageReceipt Create(IPushMessage pushMessage, IEnumerable<IPerson> receivers)
        {
            CreatedPushMessage = pushMessage;
            CreatedDialogues = new List<IPushMessageDialogue>();
            receivers.ForEach(p => CreatedDialogues.Add(new PushMessageDialogue(pushMessage, p)));
            return this;
        }

        public IList<IAggregateRoot> AddedRoots()
        {
            IList<IAggregateRoot> addedRoots = new List<IAggregateRoot> {CreatedPushMessage};
            CreatedDialogues.ForEach(addedRoots.Add);
            return addedRoots;
        }

        public IPushMessage CreatedPushMessage { get; private set; }

        public IList<IPushMessageDialogue> CreatedDialogues { get; private set; }

    }
}