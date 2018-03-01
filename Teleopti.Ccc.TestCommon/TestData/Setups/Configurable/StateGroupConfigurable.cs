using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Rta.Configuration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class StateGroupConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string PhoneState { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var stateGroup = new RtaStateGroup(Name, false, true);
			stateGroup.AddState(PhoneState, PhoneState);
			var stateGroupRepository = new RtaStateGroupRepository(currentUnitOfWork);
			stateGroupRepository.Add(stateGroup);
		}
	}
}