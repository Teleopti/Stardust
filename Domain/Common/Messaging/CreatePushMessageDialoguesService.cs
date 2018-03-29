using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.Messaging
{
    public class CreatePushMessageDialoguesService : ICreatePushMessageDialoguesService, ISendPushMessageReceipt
    {
        public ISendPushMessageReceipt Create(IPushMessage pushMessage, IEnumerable<IPerson> receivers)
        {
            CreatedPushMessage = pushMessage;
			CreatedDialogues = receivers.Select(p => (IPushMessageDialogue)new PushMessageDialogue(pushMessage, p)).ToList();
            return this;
        }

        public IList<IAggregateRoot> AddedRoots()
        {
            return new IAggregateRoot[] {CreatedPushMessage}.Concat(CreatedDialogues).ToList();
        }

        public IPushMessage CreatedPushMessage { get; private set; }

        public IList<IPushMessageDialogue> CreatedDialogues { get; private set; }

    }
}