using System;
using System.Globalization;
using System.IO;
using System.Text;
using Teleopti.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Messaging.Coders
{
    /// <summary>
    /// The external agent state decoder.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 24/06/2009
    /// </remarks>
    public class ExternalAgentStateDecoder
    {
        private readonly Encoding _encoding;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalAgentStateDecoder"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 24/06/2009
        /// </remarks>
        public ExternalAgentStateDecoder() : this(Consts.DefaultCharEncoding)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalAgentStateDecoder"/> class.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 24/06/2009
        /// </remarks>
        public ExternalAgentStateDecoder(String encoding)
        {
            _encoding = Encoding.GetEncoding(encoding);
        }

        /// <summary>
        /// Decodes the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 24/06/2009
        /// </remarks>
        public IExternalAgentState Decode(Stream source)
        {
            byte[] newline = _encoding.GetBytes(new[] {Consts.Separator});

            IFramerUtility framer = new FramerUtility();

            string externalLogOn = _encoding.GetString(framer.NextToken(source, newline));
            string stateCode = _encoding.GetString(framer.NextToken(source, newline));
            string timeInState = _encoding.GetString(framer.NextToken(source, newline));
            string timestamp = _encoding.GetString(framer.NextToken(source, newline));
            string platformTypeId = _encoding.GetString(framer.NextToken(source, newline));
            string dataSourceId = _encoding.GetString(framer.NextToken(source, newline));
            string batchId = _encoding.GetString(framer.NextToken(source, newline));
            string isSnapshot = _encoding.GetString(framer.NextToken(source, newline));

            if (externalLogOn == "-")
                externalLogOn = string.Empty;
            if (stateCode == "-")
                stateCode = string.Empty;

            return new ExternalAgentState(externalLogOn,
                                          stateCode,
                                          new TimeSpan(Convert.ToInt64(timeInState, CultureInfo.InvariantCulture)),
                                          DateTime.SpecifyKind(
                                              Convert.ToDateTime(timestamp, CultureInfo.InvariantCulture),
                                              DateTimeKind.Utc),
                                          new Guid(platformTypeId),
                                          Convert.ToInt32(dataSourceId, CultureInfo.InvariantCulture),
                                          DateTime.SpecifyKind(
                                              Convert.ToDateTime(batchId, CultureInfo.InvariantCulture),
                                              DateTimeKind.Utc),
                                          Boolean.Parse(isSnapshot));
        }

        /// <summary>
        /// Decodes the specified packet.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 24/06/2009
        /// </remarks>
        public IExternalAgentState Decode(byte[] packet)
        {
            Stream payload = new MemoryStream(packet, 0, packet.Length, false);
            return Decode(payload);
        }


    }
}