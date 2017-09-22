using System;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class GroupingReadOnlyUpdate : IDelayedSetup
	{
		public void Apply(IPerson user, ICurrentUnitOfWork currentUnitOfWork)
		{
			var groupingReadOnlyRepository = new GroupingReadOnlyRepository(CurrentUnitOfWork.Make());
			groupingReadOnlyRepository.UpdateGroupingReadModel(new Collection<Guid> { Guid.Empty });
		}
	}
}