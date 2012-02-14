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
    public class EventFilterEncoder : IEventFilterEncoder
    {
        private readonly Encoding _encoding; // Character encoding

        /// <summary>
        /// Default constructor for Event Filter Encoder.
        /// </summary>
        public EventFilterEncoder() : this(Consts.DefaultCharEncoding)
        {
        }

        /// <summary>
        /// Constructor which takes the Encoding type, ASCII by default,
        /// as input paramter.
        /// </summary>
        /// <param name="encoding"></param>
        public EventFilterEncoder(string encoding)
        {
            _encoding = Encoding.GetEncoding(encoding);
        }

        /// <summary>
        /// Encode message that takes the IEventFilter 
        /// and turns it into a byte array.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public byte[] Encode(IEventFilter item)
        {
            if (item.ChangedBy.IndexOf('\n') != -1)
                throw new IOException("Invalid ChangedBy property (contains newline)");
            
            StringBuilder sb = new StringBuilder();

            sb.Append(String.Format(CultureInfo.InvariantCulture, "{0}{1}", item.FilterId, Consts.Separator));
            sb.Append(item.SubscriberId);
            sb.Append(Consts.Separator);
            sb.Append(item.ReferenceObjectId);
            sb.Append(Consts.Separator);
            sb.Append(item.ReferenceObjectType);
            sb.Append(Consts.Separator);
            sb.Append(item.DomainObjectId);
            sb.Append(Consts.Separator);
            sb.Append(item.DomainObjectType);
            sb.Append(Consts.Separator);
            sb.Append(item.EventStartDate.ToString(CultureInfo.InvariantCulture));
            sb.Append(Consts.Separator);
            sb.Append(item.EventEndDate.ToString(CultureInfo.InvariantCulture));
            sb.Append(Consts.Separator);
            sb.Append(item.ChangedBy);
            sb.Append(Consts.Separator);
            sb.Append(item.ChangedDateTime.ToString(CultureInfo.InvariantCulture));
            sb.Append(Consts.Separator);

            string EncodedString = sb.ToString();

            if (EncodedString.Length > Consts.MaxWireLength)
                throw new IOException("Encoded length too long, max is 1024 bytes.");

            byte[] buf = _encoding.GetBytes(EncodedString);

            return buf;
        }
    }
}
