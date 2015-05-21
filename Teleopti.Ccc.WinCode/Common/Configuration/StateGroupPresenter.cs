using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
	public class StateGroupPresenter
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;


		public StateGroupPresenter(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public IList<IRtaStateGroup> LoadStateGroupCollection()
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var repository = new RtaStateGroupRepository(uow);
				return repository.LoadAllCompleteGraph();
			}
		}

		public void Save(IList<IRtaStateGroup> stateGroups, IList<IRtaStateGroup> removedGroups)
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var repository = new RtaStateGroupRepository(uow);
				foreach (var removedGroup in removedGroups.Where(x => x.Id.HasValue))
				{
					repository.Remove(removedGroup);
					removedGroup.ClearStateCodes();
				} 
				uow.Flush();

				foreach (var group in stateGroups)
				{
					repository.Add(group);
				}
				uow.PersistAll();
			}
		}

	}
}