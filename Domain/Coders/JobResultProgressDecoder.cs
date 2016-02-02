using System;
using System.Globalization;
using System.IO;
using System.Text;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Ccc.Domain.Coders
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

            var payrollResultId = _encoding.GetString(framer.NextToken(source, newline));
            var percentage = _encoding.GetString(framer.NextToken(source, newline));
            var message = _encoding.GetString(framer.NextToken(source, newline));
            var totalPercentage = _encoding.GetString(framer.NextToken(source, newline));

            if (message == "-")
                message = string.Empty;

            var jobResultProgress = new JobResultProgress
                                        {
                                            Message = message,
                                            JobResultId = new Guid(payrollResultId),
                                            Percentage = int.Parse(percentage, CultureInfo.InvariantCulture),
                                            TotalPercentage = int.Parse(totalPercentage, CultureInfo.InvariantCulture)
                                        };
               
            return jobResultProgress;
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