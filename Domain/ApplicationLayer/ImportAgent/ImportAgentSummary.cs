using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public static class ImportAgentResultSummary
	{
		public static string GetSummaryMessage(this IImportAgentResultCount count)
		{
			return $"success count:{count.SuccessCount}, failed count:{count.FaildCount}, warning count:{count.WarningCount}";
		}

		public static IImportAgentResultCount GetSummaryCount(this IJobResultDetail resultDetail)
		{
			var isMatch = new Regex("success count:\\d+, failed count:\\d+, warning count:\\d+").IsMatch(resultDetail.Message);
			if (!isMatch)
			{
				return null;
			}
			var counts = new Regex("\\d+").Matches(resultDetail.Message);
			return new ImportAgentResultCount(int.Parse(counts[0].Value), int.Parse(counts[1].Value), int.Parse(counts[2].Value));
		}
	}

	public class ImportAgentResultCount : IImportAgentResultCount
	{
		public ImportAgentResultCount(int successCount, int faildCount, int warningCount)
		{
			this.SuccessCount = successCount;
			this.FaildCount = faildCount;
			this.WarningCount = warningCount;
		}

		public int SuccessCount { get; private set; }
		public int FaildCount { get; private set; }
		public int WarningCount { get; private set; }
	}

	public interface IImportAgentResultCount
	{
		int SuccessCount { get; }
		int FaildCount { get; }
		int WarningCount { get; }
	}

}
