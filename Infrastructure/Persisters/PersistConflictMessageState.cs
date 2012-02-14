using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
    public class PersistConflictMessageState : IPersistConflict
    {
        private readonly Action<IEventMessage> _remove;
        public IPersistableScheduleData DatabaseVersion { get; private set; }
        public IEventMessage EventMessage { get; private set; }
        public DifferenceCollectionItem<IPersistableScheduleData> ClientVersion { get; private set; }

        public PersistConflictMessageState(DifferenceCollectionItem<IPersistableScheduleData> clientVersion,
                                            IPersistableScheduleData databaseVersion,
                                            IEventMessage eventMessage,
                                            Action<IEventMessage> remove)
        {
            DatabaseVersion = databaseVersion;
            EventMessage = eventMessage;
            _remove = remove;
            ClientVersion = clientVersion;
        }

        //todo: remove this delegate - just here for now
        public void RemoveFromCollection()
        {
            _remove(EventMessage);
        }
    }
}
