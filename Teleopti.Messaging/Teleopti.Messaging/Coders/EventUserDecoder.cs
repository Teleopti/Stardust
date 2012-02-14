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
    /// <summary>
    /// 
    /// </summary>
    public class EventUserDecoder : IEventUserDecoder
    {
        private readonly Encoding _encoding;

        public EventUserDecoder() : this(Consts.DefaultCharEncoding)
        {
        }

        public EventUserDecoder(String encoding)
        {
            _encoding = Encoding.GetEncoding(encoding);
        }

        public IEventUser Decode(Stream source)
        {
            byte[] newline = _encoding.GetBytes(new[] { Consts.Separator });
            IFramerUtility framer = new FramerUtility();
            string userId = _encoding.GetString(framer.NextToken(source, newline));
            string domain = _encoding.GetString(framer.NextToken(source, newline));
            string userName = _encoding.GetString(framer.NextToken(source, newline));
            string changedBy = _encoding.GetString(framer.NextToken(source, newline));
            string changedDateTime = _encoding.GetString(framer.NextToken(source, newline));
            return new EventUser(Int32.Parse(userId, CultureInfo.InvariantCulture),
                                 domain,
                                 userName,
                                 changedBy,
                                 Convert.ToDateTime(changedDateTime, CultureInfo.InvariantCulture));

        }

        public IEventUser Decode(byte[] packet)
        {
            Stream payload = new MemoryStream(packet, 0, packet.Length, false);
            return Decode(payload);
        }

    }
}
