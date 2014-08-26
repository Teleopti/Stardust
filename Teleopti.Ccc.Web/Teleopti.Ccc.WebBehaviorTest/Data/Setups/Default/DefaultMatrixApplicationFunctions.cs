using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default
{
	public class DefaultMatrixApplicationFunctions : IHashableDataSetup
	{
		private readonly string[] names =
		{
			"ResReportAbandonmentAndSpeedOfAnswer", 
			"ResReportForecastvsActualWorkload", 
			"ResReportServiceLevelAndAgentsReady", 
			"ResReportRequestsPerAgent"
		};

		public void Apply(IUnitOfWork uow)
		{
			var applicationFunctionRepository = new ApplicationFunctionRepository(uow);
			var matrixReportsParent = applicationFunctionRepository.LoadAll().First(x => x.FunctionCode == "Reports");
			
			applicationFunctionRepository.AddRange(
				names.Select(n => new ApplicationFunction(n, matrixReportsParent) { ForeignSource = DefinedForeignSourceNames.SourceMatrix }));
		}

		public int HashValue()
		{
			return names.Aggregate(37, (current, name) => current ^ name.GetHashCode());
		}
	}
}