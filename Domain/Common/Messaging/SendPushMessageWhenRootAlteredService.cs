using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Common.Messaging
{
    public class SendPushMessageWhenRootAlteredService:ISendPushMessageWhenRootAlteredService
    {
        private static ILog Logger = LogManager.GetLogger(typeof (SendPushMessageWhenRootAlteredService));

        public SendPushMessageWhenRootAlteredService()
        {
            SendPushMessagesWhenRootAltered = new List<IPushMessageWhenRootAltered>();
        }
        
        public void AddAlteredRoots(IEnumerable<IRootChangeInfo> changedRoots)
        {
            foreach (var rootChangedInfo in changedRoots)
            {
                IPushMessageWhenRootAltered pushMessageWhenRootAltered =
                    rootChangedInfo.Root as IPushMessageWhenRootAltered;
                if(pushMessageWhenRootAltered!=null)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.DebugFormat("Up for sending message for {0}",pushMessageWhenRootAltered);
                    }
                    SendPushMessagesWhenRootAltered.Add(pushMessageWhenRootAltered);
                }
            }
        }

        public IList<IAggregateRoot> SendPushMessages(IPushMessagePersister repository)
        {
            IList<IAggregateRoot> addedRoots = new List<IAggregateRoot>();

            foreach (var altered in SendPushMessagesWhenRootAltered)
            {
                if(altered.ShouldSendPushMessageWhenAltered())
                {
                    ISendPushMessageService sendPushMessageService =altered.PushMessageWhenAlteredInformation();
                    if(sendPushMessageService!=null)
                    {
                       ISendPushMessageReceipt receipt= sendPushMessageService.SendConversationWithReceipt(repository);
                        if(receipt!=null)
                        {
                            receipt.AddedRoots().ForEach(addedRoots.Add);
                        }

                    }
                }
            }

            return addedRoots;
        }


		public IList<IAggregateRoot> SendPushMessages(IEnumerable<IRootChangeInfo> changedRoots, IPushMessagePersister repository)
        {
            SendPushMessagesWhenRootAltered.Clear();
            AddAlteredRoots(changedRoots);
            
            return SendPushMessages(repository);
        }

        public IList<IPushMessageWhenRootAltered> SendPushMessagesWhenRootAltered { get; private set; }
      
    }
}
