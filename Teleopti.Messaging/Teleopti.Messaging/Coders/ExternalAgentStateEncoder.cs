using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Messaging.Coders
{
    /// <summary>
    /// The external agent state encoder.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 24/06/2009
    /// </remarks>
    public class ExternalAgentStateEncoder
    {
        private readonly Encoding _encoding; // Character encoding

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalAgentStateEncoder"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 24/06/2009
        /// </remarks>
        public ExternalAgentStateEncoder() : this(Consts.DefaultCharEncoding)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalAgentStateEncoder"/> class.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 24/06/2009
        /// </remarks>
        public ExternalAgentStateEncoder(string encoding)
        {
            _encoding = Encoding.GetEncoding(encoding);
        }

        /// <summary>
        /// Encodes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 24/06/2009
        /// </remarks>
        public byte[] Encode(IExternalAgentState item)
        {
            StringBuilder sb = new StringBuilder();

            string externalLogOn = item.ExternalLogOn;
            if (string.IsNullOrEmpty(externalLogOn))
                externalLogOn = "-";

            string stateCode = item.StateCode;
            if (string.IsNullOrEmpty(stateCode))
                stateCode = "-";

            sb.Append(externalLogOn);
            sb.Append(Consts.Separator);
            sb.Append(stateCode);
            sb.Append(Consts.Separator);
            sb.Append(item.TimeInState.Ticks.ToString(CultureInfo.InvariantCulture));
            sb.Append(Consts.Separator);
            sb.Append(item.Timestamp.ToString(CultureInfo.InvariantCulture));
            sb.Append(Consts.Separator);
            sb.Append(item.PlatformTypeId);
            sb.Append(Consts.Separator);
            sb.Append(item.DataSourceId);
            sb.Append(Consts.Separator);
            sb.Append(item.BatchId.ToString(CultureInfo.InvariantCulture));
            sb.Append(Consts.Separator);
            sb.Append(item.IsSnapshot);
            sb.Append(Consts.Separator);

            List<Byte> encodedList = new List<byte>();
            encodedList.AddRange(_encoding.GetBytes(sb.ToString()));
            
            byte[] buf = encodedList.ToArray();
            return buf;

        }
    }
}