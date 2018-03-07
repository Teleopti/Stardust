using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public static class JobResultExtensions
	{
		public static void RethrowIfException(this IJobResult jobResult)
		{
			if (jobResult.FinishedOk)
				return;

			var exMEssage = "Exception during job!" + Environment.NewLine + detailExceptions(jobResult);
			
			throw new JobResultException(exMEssage);
		}

		private static string detailExceptions(IJobResult jobResult)
		{
			string exString = null;

			foreach (var detail in jobResult.Details)
			{
				var exMessageForDetail = detail.ExceptionMessage;
				if (!string.IsNullOrEmpty(exMessageForDetail))
					exString += exMessageForDetail + Environment.NewLine;
			}

			return exString ?? 
				   "Don't have any info what the exceptions are unfortunatly (probably failed to persist jobresult detail)... You'll need to debug, I guess.";
		}
	}

	public class JobResultException : Exception
	{
		public JobResultException(string message) : base(message)
		{	
		}
	}
}