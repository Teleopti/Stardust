using System.Globalization;
using System.IO;
using System.Text;
using Teleopti.Interfaces.MessageBroker.Coders;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Coders
{
    public class EventHeartbeatEncoder : IEventHeartbeatEncoder
    {
        private readonly Encoding _encoding;

        public EventHeartbeatEncoder() : this(Consts.DefaultCharEncoding)
        {
        }

        public EventHeartbeatEncoder(string encoding)
        {
            _encoding = Encoding.GetEncoding(encoding);
        }

        public byte[] Encode(IEventHeartbeat item)
        {
            if (item.ChangedBy.IndexOf('\n') != -1)
                throw new IOException("Invalid ChangedBy property (contains newline)");

            StringBuilder sb = new StringBuilder();

            sb.Append(item.HeartbeatId);
            sb.Append(Consts.Separator);
            sb.Append(item.SubscriberId);
            sb.Append(Consts.Separator);
            sb.Append(item.ProcessId);
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
