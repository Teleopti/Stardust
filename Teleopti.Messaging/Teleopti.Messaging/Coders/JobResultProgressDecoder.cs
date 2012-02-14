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
    /// The job result progress decoder.
    /// </summary>
    public class JobResultProgressDecoder
    {
        private readonly Encoding _encoding;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobResultProgressDecoder"/> class.
        /// </summary>
        public JobResultProgressDecoder() : this(Consts.DefaultCharEncoding)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobResultProgressDecoder"/> class.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        public JobResultProgressDecoder(String encoding)
        {
            _encoding = Encoding.GetEncoding(encoding);
        }

        /// <summary>
        /// Decodes the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public IJobResultProgress Decode(Stream source)
        {
            byte[] newline = _encoding.GetBytes(new[] {Consts.Separator});

            IFramerUtility framer = new FramerUtility();

            string payrollResultId = _encoding.GetString(framer.NextToken(source, newline));
            string percentage = _encoding.GetString(framer.NextToken(source, newline));
            string message = _encoding.GetString(framer.NextToken(source, newline));
            
            if (message == "-")
                message = string.Empty;

            return new JobResultProgress
                       {
                           Message = message,
                           JobResultId = new Guid(payrollResultId),
                           Percentage = int.Parse(percentage, CultureInfo.InvariantCulture)
                       };
        }

        /// <summary>
        /// Decodes the specified packet.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <returns></returns>
        public IJobResultProgress Decode(byte[] packet)
        {
            Stream payload = new MemoryStream(packet, 0, packet.Length, false);
            return Decode(payload);
        }
    }
}