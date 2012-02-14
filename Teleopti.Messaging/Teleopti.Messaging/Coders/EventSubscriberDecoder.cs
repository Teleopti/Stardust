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
    public class EventSubscriberDecoder : IEventSubscriberDecoder
    {
        private readonly Encoding _encoding;

        /// <summary>
        /// Default constructor which sends in ASCII to the
        /// specific constructor.
        /// </summary>
        public EventSubscriberDecoder() : this(Consts.DefaultCharEncoding)
        {
        }

        /// <summary>
        /// Specific EventSubscriberDecoder constructor which takes the Encoding standard
        /// as input parameter upon construction of a new object. The default Encoding standard
        /// id ASCII. 
        /// </summary>
        /// <param name="encoding"></param>
        public EventSubscriberDecoder(String encoding)
        {
            _encoding = Encoding.GetEncoding(encoding);
        }

        /// <summary>
        /// Decode method decodes a stream to an IEventSubscriber.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public IEventSubscriber Decode(Stream source)
        {
            byte[] newline = _encoding.GetBytes(new char[] { Consts.Separator });
            IFramerUtility framer = new FramerUtility();
            string subscriberId = _encoding.GetString(framer.NextToken(source, newline));
            string userId = _encoding.GetString(framer.NextToken(source, newline));
            string processId = _encoding.GetString(framer.NextToken(source, newline));
            string ipAddress = _encoding.GetString(framer.NextToken(source, newline));
            string port = _encoding.GetString(framer.NextToken(source, newline));
            string changedBy = _encoding.GetString(framer.NextToken(source, newline));
            string changedDateTime = _encoding.GetString(framer.NextToken(source, newline));
            Guid subscriberGuid = new Guid(subscriberId);
            return new EventSubscriber(subscriberGuid,
                                       Int32.Parse(userId, CultureInfo.InvariantCulture),
                                       Int32.Parse(processId, CultureInfo.InvariantCulture),
                                       ipAddress,
                                       Int32.Parse(port, CultureInfo.InvariantCulture),
                                       changedBy,
                                       Convert.ToDateTime(changedDateTime, CultureInfo.InvariantCulture));

        }

        /// <summary>
        /// The overloaded Decode method takes a byte array transform it to a stream
        /// and sends it in to the overloaded method above and returns an IEventSubscriber.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public IEventSubscriber Decode(byte[] packet)
        {
            Stream payload = new MemoryStream(packet, 0, packet.Length, false);
            return Decode(payload);
        }

    }
}
