using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Wfm.Adherence.Configuration;
using Teleopti.Wfm.Adherence.Configuration.Repositories;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
	public class StateGroupPersistHelper
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public StateGroupPersistHelper(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public IList<IRtaStateGroup> LoadStateGroupCollection()
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var repository = new RtaStateGroupRepository(new ThisUnitOfWork(uow));
				return repository.LoadAllCompleteGraph();
			}
		}

		// Forgive me!
		public void Save(IList<IRtaStateGroup> stateGroups, IList<IRtaStateGroup> removedGroups,
			IList<IRtaStateGroup> groupsWithRemovedStates)
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var stateGroupRepository = new RtaStateGroupRepository(new ThisUnitOfWork(uow));
				var alarmMappingRepository = new RtaMapRepository(uow);
				foreach (var removedGroup in removedGroups.Where(x => x.Id.HasValue))
				{
					removeGroupActivityAlarmMappingsWithGroup(removedGroup, alarmMappingRepository);
					stateGroupRepository.Remove(removedGroup);
					removedGroup.ClearStates();
				}
				// Groups with removed states has to be flushed before persist
				foreach (var state in groupsWithRemovedStates.Where(x => !removedGroups.Contains(x)))
					stateGroupRepository.Add(state);
				uow.Flush();

				foreach (var group in stateGroups)
					stateGroupRepository.Add(group);
				uow.PersistAll();
			}
		}

		private static void removeGroupActivityAlarmMappingsWithGroup(IRtaStateGroup @group, IRepository<IRtaMap> alarmMappingRepository)
		{
			var mappingsWithStateGroup = alarmMappingRepository.LoadAll()
				.Where(x => @group.Equals(x.StateGroup));
			foreach (var matchingMapping in mappingsWithStateGroup)
				alarmMappingRepository.Remove(matchingMapping);
		}
	}
}