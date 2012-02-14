using System;
using System.Globalization;
using System.IO;
using System.Text;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Coders;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;

namespace Teleopti.Messaging.Coders
{
    public class EventFilterDecoder : IEventFilterDecoder
    {
        private readonly Encoding _encoding;

        public EventFilterDecoder() : this(Consts.DefaultCharEncoding)
        {
        }

        public EventFilterDecoder(String encoding)
        {
            _encoding = Encoding.GetEncoding(encoding);
        }

        public IEventFilter Decode(Stream source)
        {
            byte[] newline = _encoding.GetBytes(new[] { Consts.Separator });

            IFramerUtility framer = new FramerUtility();

            string filterId = _encoding.GetString(framer.NextToken(source, newline));
            string subscriberId = _encoding.GetString(framer.NextToken(source, newline));
            string parentObjectId = _encoding.GetString(framer.NextToken(source, newline));
            string parentObjectType = _encoding.GetString(framer.NextToken(source, newline));
            string domainObjectId = _encoding.GetString(framer.NextToken(source, newline));
            string domainObjectType = _encoding.GetString(framer.NextToken(source, newline));
            string eventStartDate = _encoding.GetString(framer.NextToken(source, newline));
            string eventEndDate = _encoding.GetString(framer.NextToken(source, newline));
            string changedBy = _encoding.GetString(framer.NextToken(source, newline));
            string changedDateTime = _encoding.GetString(framer.NextToken(source, newline));
            
            Guid filterGuid = new Guid(filterId);
            Guid subscriberGuid = new Guid(subscriberId);
            Guid parentObjectGuid = new Guid(parentObjectId);
            Guid domainObjectGuid = new Guid(domainObjectId);

            return new EventFilter( filterGuid,
                                    subscriberGuid,
                                    parentObjectGuid,
                                    parentObjectType,
                                    domainObjectGuid,
                                    domainObjectType,
                                    Convert.ToDateTime(eventStartDate, CultureInfo.InvariantCulture),
                                    Convert.ToDateTime(eventEndDate, CultureInfo.InvariantCulture),
                                    changedBy,
                                    Convert.ToDateTime(changedDateTime, CultureInfo.InvariantCulture));

        }

        public IEventFilter Decode(byte[] packet)
        {
            Stream payload = new MemoryStream(packet, 0, packet.Length, false);
            return Decode(payload);
        }
    }
}