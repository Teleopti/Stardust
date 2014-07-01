using System;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class StateGroupConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string PhoneState { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			var stateGroup = new RtaStateGroup(Name, false, true);
			stateGroup.AddState(PhoneState, PhoneState, Guid.Empty);
			var stateGroupRepository = new RtaStateGroupRepository(uow);
			stateGroupRepository.Add(stateGroup);
		}
	}
}