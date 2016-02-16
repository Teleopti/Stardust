using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Coders
{
    /// <summary>
    /// The job result progress encoder.
    /// </summary>
    public class JobResultProgressEncoder
    {
        private readonly Encoding _encoding; // Character encoding

        /// <summary>
        /// Initializes a new instance of the <see cref="JobResultProgressEncoder"/> class.
        /// </summary>
        public JobResultProgressEncoder() : this(Consts.DefaultCharEncoding)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobResultProgressEncoder"/> class.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        public JobResultProgressEncoder(string encoding)
        {
            _encoding = Encoding.GetEncoding(encoding);
        }

        /// <summary>
        /// Encodes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public byte[] Encode(IJobResultProgress item)
        {
            var sb = new StringBuilder();

            var message = item.Message;
            if (string.IsNullOrEmpty(message))
                message = "-";

            sb.Append(item.JobResultId);
            sb.Append(Consts.Separator);
            sb.Append(item.Percentage.ToString(CultureInfo.InvariantCulture));
            sb.Append(Consts.Separator);
            sb.Append(message);
            sb.Append(Consts.Separator);
            sb.Append(item.TotalPercentage.ToString(CultureInfo.InvariantCulture));

            var encodedList = new List<byte>();
            encodedList.AddRange(_encoding.GetBytes(sb.ToString()));
            
            byte[] buf = encodedList.ToArray();
            return buf;

        }
    }
}