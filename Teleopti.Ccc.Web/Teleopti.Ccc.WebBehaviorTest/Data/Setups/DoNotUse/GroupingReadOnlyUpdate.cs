using System;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class GroupingReadOnlyUpdate : IDelayedSetup
	{
		public void Apply(IPerson user, IUnitOfWork uow)
		{
			var uowf = GlobalUnitOfWorkState.CurrentUnitOfWorkFactory;
			var groupingReadOnlyRepository = new GroupingReadOnlyRepository(uowf);
			groupingReadOnlyRepository.UpdateGroupingReadModel(new Collection<Guid> { Guid.Empty });
		}
	}
}