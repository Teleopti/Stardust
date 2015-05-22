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

		public void Save(IList<IRtaStateGroup> stateGroups, IList<IRtaStateGroup> removedGroups,
			IList<IRtaStateGroup> groupsWithRemovedStates)
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var stateGroupRepository = new RtaStateGroupRepository(uow);
				var alarmMappingRepository = new StateGroupActivityAlarmRepository(uow);
				foreach (var removedGroup in removedGroups.Where(x => x.Id.HasValue))
				{
					var group = removedGroup;
					var mappingsWithStateGroup = alarmMappingRepository.LoadAll();
					foreach (var matchingMapping in mappingsWithStateGroup
						.Where(x => x.StateGroup != null &&
						            x.StateGroup.Id.Value == group.Id.Value))
						alarmMappingRepository.Remove(matchingMapping);

					stateGroupRepository.Remove(removedGroup);
					removedGroup.ClearStates();
				}
				foreach (var state in groupsWithRemovedStates.Where(x => !removedGroups.Contains(x)))
					stateGroupRepository.Add(state);
				uow.Flush();

				foreach (var group in stateGroups)
					stateGroupRepository.Add(group);
				uow.PersistAll();
			}
		}
	}
}