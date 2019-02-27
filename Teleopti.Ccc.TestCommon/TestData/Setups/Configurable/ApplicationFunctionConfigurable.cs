using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class ApplicationFunctionConfigurable : IDataSetup
	{

		public string Name { get; set; }
		public string LocalizedFunctionDescription { get; set; }
		public IApplicationFunction Function;
		
		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			Function = new ApplicationFunction(Name);
			var applicationFunctionRepository = ApplicationFunctionRepository.DONT_USE_CTOR(currentUnitOfWork);
			applicationFunctionRepository.Add(Function);
		}
	}
}