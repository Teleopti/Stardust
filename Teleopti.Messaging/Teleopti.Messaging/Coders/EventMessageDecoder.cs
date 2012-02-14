using System;
using System.Globalization;
using System.Text;  
using System.IO;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Coders;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;

namespace Teleopti.Messaging.Coders
{
    /// <summary>
    /// 
    /// </summary>
    public class EventMessageDecoder : IEventMessageDecoder
    {
        private readonly Encoding _encoding; 

        public EventMessageDecoder() : this(Consts.DefaultCharEncoding)
        {
        }

        public EventMessageDecoder(String encoding)
        {
            _encoding = Encoding.GetEncoding(encoding);
        }

        public IEventMessage Decode(Stream source)
        {
            byte[] newline = _encoding.GetBytes(new char[]{Consts.Separator});

            IFramerUtility framer = new FramerUtility();

            string eventID = _encoding.GetString(framer.NextToken(source, newline));
            string eventStartDate = _encoding.GetString(framer.NextToken(source, newline));
            string eventEndDate = _encoding.GetString(framer.NextToken(source, newline));
            string userID = _encoding.GetString(framer.NextToken(source, newline));
            string processId = _encoding.GetString(framer.NextToken(source, newline));
            string moduleId = _encoding.GetString(framer.NextToken(source, newline));
            string packageSize = _encoding.GetString(framer.NextToken(source, newline));
            string isHeartbeat = _encoding.GetString(framer.NextToken(source, newline));
            string referenceObjectID = _encoding.GetString(framer.NextToken(source, newline));
            string referenceObjectType = _encoding.GetString(framer.NextToken(source, newline));
            string domainObjectID = _encoding.GetString(framer.NextToken(source, newline));
            string domainObjectType = _encoding.GetString(framer.NextToken(source, newline));
            string domainUpdateType = _encoding.GetString(framer.NextToken(source, newline));
            string changedBy = _encoding.GetString(framer.NextToken(source, newline));
            string changedDateTime = _encoding.GetString(framer.NextToken(source, newline));
            byte[] domainObject = framer.ReadEnd(source);

            Guid eventGuid = new Guid(eventID);
            Guid referenceObjectGuid = new Guid(referenceObjectID);
            Guid domainObjectGuid = new Guid(domainObjectID);
            Guid moduleGuid = new Guid(moduleId);

            return new EventMessage(eventGuid,
                                    Convert.ToDateTime(eventStartDate, CultureInfo.InvariantCulture),
                                    Convert.ToDateTime(eventEndDate, CultureInfo.InvariantCulture),
                                    Int32.Parse(userID, CultureInfo.InvariantCulture),
                                    Int32.Parse(processId, CultureInfo.InvariantCulture),
                                    moduleGuid,
                                    Int32.Parse(packageSize, CultureInfo.InvariantCulture),
                                    Boolean.Parse(isHeartbeat),
                                    referenceObjectGuid,
                                    referenceObjectType,
                                    domainObjectGuid,
                                    domainObjectType,
                                    (DomainUpdateType) Int32.Parse(domainUpdateType, CultureInfo.InvariantCulture),
                                    domainObject,
                                    changedBy,
                                    Convert.ToDateTime(changedDateTime, CultureInfo.InvariantCulture));

        }

        public IEventMessage Decode(byte[] packet)
        {
            Stream payload = new MemoryStream(packet, 0, packet.Length, false);
            return Decode(payload);
        }

    }
}