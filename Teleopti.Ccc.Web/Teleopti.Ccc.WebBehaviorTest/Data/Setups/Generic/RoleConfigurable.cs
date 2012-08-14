using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class RoleConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string BusinessUnit { get; set; }
		public bool ViewUnpublishedSchedules { get; set; }
		public bool ViewConfidential { get; set; }
		public bool AccessToMobileReports { get; set; }

		public RoleConfigurable()
		{
			BusinessUnit = GlobalDataContext.Data().Data<CommonBusinessUnit>().BusinessUnit.Description.Name;
			ViewUnpublishedSchedules = false;
			ViewConfidential = false;
			AccessToMobileReports = true;
		}

		public void Apply(IUnitOfWork uow)
		{
			var applicationFunctionRepository = new ApplicationFunctionRepository(uow);
			var allApplicationFunctions = applicationFunctionRepository.GetAllApplicationFunctionSortedByCode().AsEnumerable();

			var applicationFunctions = from f in allApplicationFunctions where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.All select f;

			if (!ViewUnpublishedSchedules)
				applicationFunctions = from f in applicationFunctions where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules select f;

			if (!ViewConfidential)
				applicationFunctions = from f in applicationFunctions where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.ViewConfidential select f;

			if (!AccessToMobileReports)
				applicationFunctions = from f in applicationFunctions where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.Anywhere select f;

			var role = ApplicationRoleFactory.CreateRole(Name, null);

			var availableData = new AvailableData
			                    	{
			                    		ApplicationRole = role,
			                    		AvailableDataRange = AvailableDataRangeOption.MyTeam
			                    	};

			role.AvailableData = availableData;

			var businessUnitRepository = new BusinessUnitRepository(uow);
			var businessUnit = businessUnitRepository.LoadAllBusinessUnitSortedByName().Single(b => b.Name == BusinessUnit);
			role.SetBusinessUnit(businessUnit);
			applicationFunctions.ToList().ForEach(role.AddApplicationFunction);

			var applicationRoleRepository = new ApplicationRoleRepository(uow);
			var availableDataRepository = new AvailableDataRepository(uow);

			applicationRoleRepository.Add(role);
			availableDataRepository.Add(availableData);

		}
	}
}