﻿using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

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
				if(rootChangedInfo.Root is IPushMessageWhenRootAltered pushMessageWhenRootAltered)
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
					ISendPushMessageReceipt receipt= sendPushMessageService?.SendConversationWithReceipt(repository);
					receipt?.AddedRoots().ForEach(addedRoots.Add);
				}
            }

            return addedRoots;
        }


		public IList<IAggregateRoot> SendPushMessages(IEnumerable<IRootChangeInfo> changedRoots, IPushMessagePersister repository)
        {
            SendPushMessagesWhenRootAltered.Clear();
			var filteredChangedRoots = changedRoots.Where(changedRoot => changedRoot.Status != DomainUpdateType.Insert);
            AddAlteredRoots(filteredChangedRoots);
            
            return SendPushMessages(repository);
        }

        public IList<IPushMessageWhenRootAltered> SendPushMessagesWhenRootAltered { get; private set; }
      
    }
}
