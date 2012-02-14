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
    public class EventHeartbeatDecoder : IEventHeartbeatDecoder
    {
        private readonly Encoding _encoding;

        public EventHeartbeatDecoder() : this(Consts.DefaultCharEncoding)
        {
        }

        public EventHeartbeatDecoder(String encoding)
        {
            _encoding = Encoding.GetEncoding(encoding);
        }

        public IEventHeartbeat Decode(Stream source)
        {
            byte[] newline = _encoding.GetBytes(new char[] { Consts.Separator });
            IFramerUtility framer = new FramerUtility();
            string HeartbeatId = _encoding.GetString(framer.NextToken(source, newline));
            string SubscriberId = _encoding.GetString(framer.NextToken(source, newline));
            string ProcessId = _encoding.GetString(framer.NextToken(source, newline));
            string changedBy = _encoding.GetString(framer.NextToken(source, newline));
            string changedDateTime = _encoding.GetString(framer.NextToken(source, newline));
            Guid guid = new Guid(HeartbeatId);
            Guid subscriberGuid = new Guid(SubscriberId);
            return new EventHeartbeat(  guid,
                                        subscriberGuid,
                                        Int32.Parse(ProcessId, CultureInfo.InvariantCulture),
                                        changedBy,
                                        Convert.ToDateTime(changedDateTime, CultureInfo.InvariantCulture));

        }

        public IEventHeartbeat Decode(byte[] packet)
        {
            Stream payload = new MemoryStream(packet, 0, packet.Length, false);
            return Decode(payload);
        }

    }
}
