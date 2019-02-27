using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Default
{
	public class DefaultMatrixApplicationFunctions : IHashableDataSetup
	{
		private readonly string[] names =
		{
			"ResReportAgentQueueMetrics",
			"ResReportAbandonmentAndSpeedOfAnswer", 
			"ResReportForecastvsActualWorkload", 
			"ResReportServiceLevelAndAgentsReady", 
			"ResReportRequestsPerAgent"
		};
		// guids from mart.report with these names
		private readonly string[] guids =
		{
			"479809D8-4DAE-4852-BF67-C98C3744918D",
			"C232D751-AEC5-4FD7-A274-7C56B99E8DEC", 
			"8D8544E4-6B24-4C1C-8083-CBE7522DD0E0", 
			"AE758403-C16B-40B0-B6B2-E8F6043B6E04", 
			"8DE1AB0F-32C2-4619-A2B2-97385BE4C49C"
		};

			
//xxResReportScheduledOvertimePerAgent	EB977F5B-86C6-4D98-BEDF-B79DC562987B
		private string reportGuid(string name)
		{
			var index = Array.IndexOf(names, name);
			return guids[index];
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var applicationFunctionRepository = ApplicationFunctionRepository.DONT_USE_CTOR(currentUnitOfWork);
			var matrixReportsParent = applicationFunctionRepository.LoadAll().First(x => x.FunctionCode == "Reports");

			applicationFunctionRepository.AddRange(
				names.Select(n => new ApplicationFunction(n, matrixReportsParent) { ForeignSource = DefinedForeignSourceNames.SourceMatrix, ForeignId = reportGuid(n) }));
		}

		public int HashValue()
		{
			return names.Aggregate(37, (current, name) => current ^ name.GetHashCode());
		}
	}
}