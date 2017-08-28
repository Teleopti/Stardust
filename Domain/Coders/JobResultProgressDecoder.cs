using System;
using System.Globalization;
using System.IO;
using System.Text;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;

namespace Teleopti.Ccc.Domain.Coders
{
	/// <summary>
	/// The job result progress decoder.
	/// </summary>
	public class JobResultProgressDecoder
	{
		private readonly Encoding _encoding;
		private static readonly ILog logger = LogManager.GetLogger(typeof(JobResultProgressDecoder));
		private readonly IFramerUtility framer = new FramerUtility();
		
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
			byte[] newline = _encoding.GetBytes(new[] { Consts.Separator });

			var payrollResultId = _encoding.GetString(framer.NextToken(source, newline));
			var percentage = _encoding.GetString(framer.NextToken(source, newline));
			var message = _encoding.GetString(framer.NextToken(source, newline));
			var totalPercentage = _encoding.GetString(framer.NextToken(source, newline));

			if (message == "-")
				message = string.Empty;

			int jobResultProgressPercentage;
			if (!int.TryParse(percentage, NumberStyles.Any, CultureInfo.InvariantCulture, out jobResultProgressPercentage))
			{
				jobResultProgressPercentage = 0;
				logger.Debug($"Could not parse int {nameof(percentage)} from string '{percentage}', defaults to {jobResultProgressPercentage}");
			}

			int jobReusltProgressTotalPercentage;
			if (!int.TryParse(totalPercentage, NumberStyles.Any, CultureInfo.InvariantCulture, out jobReusltProgressTotalPercentage))
			{
				jobReusltProgressTotalPercentage = 0;
				logger.Debug($"Could not parse int {nameof(totalPercentage)} from string '{totalPercentage}', defaults to {jobReusltProgressTotalPercentage}");
			}

			var jobResultProgress = new JobResultProgress
			{
				Message = message,
				JobResultId = new Guid(payrollResultId),
				Percentage = jobResultProgressPercentage,
				TotalPercentage = jobReusltProgressTotalPercentage
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