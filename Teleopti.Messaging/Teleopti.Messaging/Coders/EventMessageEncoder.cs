using System;
using System.Collections.Generic;
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
    public class EventMessageEncoder : IEventMessageEncoder
    {
        private readonly Encoding _encoding; // Character encoding

        public EventMessageEncoder() : this(Consts.DefaultCharEncoding)
        {
        }

        public EventMessageEncoder(string encoding)
        {
            _encoding = Encoding.GetEncoding(encoding);
        }

        public byte[] Encode(IEventMessage item)
        {

            if (item.ChangedBy.IndexOf('\n') != -1)
                throw new IOException("Invalid ChangedBy property (contains newline)");

            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format(CultureInfo.InvariantCulture, "{0}{1}", item.EventId, Consts.Separator));
            sb.Append(item.EventStartDate.ToString(CultureInfo.InvariantCulture));
            sb.Append(Consts.Separator);
            sb.Append(item.EventEndDate.ToString(CultureInfo.InvariantCulture));
            sb.Append(Consts.Separator);
            sb.Append(item.UserId);
            sb.Append(Consts.Separator);
            sb.Append(item.ProcessId);
            sb.Append(Consts.Separator);
            sb.Append(item.ModuleId);
            sb.Append(Consts.Separator);
            sb.Append(item.PackageSize);
            sb.Append(Consts.Separator);
            sb.Append(item.IsHeartbeat);
            sb.Append(Consts.Separator);
            sb.Append(item.ReferenceObjectId);
            sb.Append(Consts.Separator);
            sb.Append(item.ReferenceObjectType);
            sb.Append(Consts.Separator);
            sb.Append(item.DomainObjectId);
            sb.Append(Consts.Separator);
            sb.Append(item.DomainObjectType);
            sb.Append(Consts.Separator);
            sb.Append(Convert.ToInt32(item.DomainUpdateType,CultureInfo.InvariantCulture));
            sb.Append(Consts.Separator);
            sb.Append(item.ChangedBy);
            sb.Append(Consts.Separator);
            sb.Append(item.ChangedDateTime.ToString(CultureInfo.InvariantCulture));
            sb.Append(Consts.Separator);

            List<Byte> encodedList = new List<byte>();
            encodedList.AddRange(_encoding.GetBytes(sb.ToString()));
            encodedList.AddRange(item.DomainObject);
            if (encodedList.Count > Consts.MaxWireLength)
                throw new IOException("Encoded length too long, max is 1024 bytes.");

            byte[] buf = encodedList.ToArray();
            return buf;

        }

    }
}