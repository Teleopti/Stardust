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
    public class EventUserEncoder : IEventUserEncoder
    {
        private readonly Encoding _encoding;

        public EventUserEncoder() : this(Consts.DefaultCharEncoding)
        {
        }

        public EventUserEncoder(string encoding)
        {
            _encoding = Encoding.GetEncoding(encoding);
        }

        public byte[] Encode(IEventUser item)
        {
            if (item.ChangedBy.IndexOf('\n') != -1)
                throw new IOException("Invalid ChangedBy property (contains newline)");
            if (item.Domain.IndexOf('\n') != -1)
                throw new IOException("Invalid Domain property (contains newline)");
            if (item.UserName.IndexOf('\n') != -1)
                throw new IOException("Invalid UserName property (contains newline)");

            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format(CultureInfo.InvariantCulture, "{0}{1}", item.UserId, Consts.Separator));
            sb.Append(item.Domain);
            sb.Append(Consts.Separator);
            sb.Append(item.UserName);
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
