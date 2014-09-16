using System;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class GroupingReadOnlyUpdate : IDelayedSetup
	{
		public void Apply(IPerson user, IUnitOfWork uow)
		{
			var groupingReadOnlyRepository = new GroupingReadOnlyRepository(CurrentUnitOfWork.Make());
			groupingReadOnlyRepository.UpdateGroupingReadModel(new Collection<Guid> { Guid.Empty });
		}
	}
}