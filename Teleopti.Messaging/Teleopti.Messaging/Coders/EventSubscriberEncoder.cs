using System;
using System.Globalization;
using System.IO;
using System.Text;
using Teleopti.Interfaces.MessageBroker.Coders;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Coders
{
    /// <summary>
    /// 
    /// </summary>
    public class EventSubscriberEncoder : IEventSubscriberEncoder
    {
        private readonly Encoding _encoding; // Character encoding

        /// <summary>
        /// Default constructor
        /// </summary>
        public EventSubscriberEncoder() : this(Consts.DefaultCharEncoding)
        {

        }

        /// <summary>
        /// Takes a specific encoding standard upon construction.
        /// 
        /// </summary>
        /// <param name="encoding"></param>
        public EventSubscriberEncoder(string encoding)
        {
            _encoding = Encoding.GetEncoding(encoding);
        }

        /// <summary>
        /// Encodes an EventSubscriber object to a byte array.
        /// This method can throw an IOException upon invalid
        /// EventSubscriber property data.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public byte[] Encode(IEventSubscriber item)
        {
            if (item.ChangedBy.IndexOf('\n') != -1)
                throw new IOException("Invalid ChangedBy property (contains newline)");

            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format(CultureInfo.InvariantCulture, "{0}{1}", item.SubscriberId, Consts.Separator));
            
            sb.Append(item.UserId);
            sb.Append(Consts.Separator);
            sb.Append(item.ProcessId);
            sb.Append(Consts.Separator);
            sb.Append(item.IPAddress);
            sb.Append(Consts.Separator);
            sb.Append(item.Port);
            sb.Append(Consts.Separator);
            sb.Append(item.ChangedBy);
            sb.Append(Consts.Separator);
            sb.Append(item.ChangedDateTime.ToString(CultureInfo.InvariantCulture));

            string EncodedString = sb.ToString();
            if (EncodedString.Length > Consts.MaxWireLength)
                throw new IOException("Encoded length too long, max is 1024 bytes.");

            byte[] buf = _encoding.GetBytes(EncodedString);
            return buf;

        }

    }
}
