using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class MultiplicatorDefinitionSetConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public IMultiplicatorDefinitionSet MultiplicatorDefinitionSet { get; private set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var multiplicatorDefinitionSetRepository = new MultiplicatorDefinitionSetRepository(currentUnitOfWork);
			MultiplicatorDefinitionSet = new MultiplicatorDefinitionSet(Name, MultiplicatorType.Overtime);
			multiplicatorDefinitionSetRepository.Add(MultiplicatorDefinitionSet);
		}
	}
}