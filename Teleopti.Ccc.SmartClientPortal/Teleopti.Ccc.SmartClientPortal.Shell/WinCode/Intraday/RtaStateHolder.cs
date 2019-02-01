using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Configuration;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday
{
    public class RtaStateHolder : IRtaStateHolder
    {
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly IRtaStateGroupRepository _rtaStateGroupRepository;
		private readonly ConcurrentDictionary<Guid, AgentStateReadModel> _actualAgentStates = new ConcurrentDictionary<Guid, AgentStateReadModel>();
		
        public RtaStateHolder(ISchedulingResultStateHolder schedulingResultStateHolder, IRtaStateGroupRepository rtaStateGroupRepository)
        {
            InParameter.NotNull(nameof(schedulingResultStateHolder), schedulingResultStateHolder);
            InParameter.NotNull(nameof(rtaStateGroupRepository), rtaStateGroupRepository);

            _schedulingResultStateHolder = schedulingResultStateHolder;
            _rtaStateGroupRepository = rtaStateGroupRepository;
        }

        public void Initialize()
        {
            RtaStateGroups = _rtaStateGroupRepository.LoadAllCompleteGraph();
        }

	    public IEnumerable<IRtaStateGroup> RtaStateGroups { get; private set; }


	    public ISchedulingResultStateHolder SchedulingResultStateHolder => _schedulingResultStateHolder;

	    public event EventHandler<CustomEventArgs<AgentStateReadModel>> AgentstateUpdated;

        public IDictionary<Guid, AgentStateReadModel> ActualAgentStates => new ReadOnlyDictionary<Guid, AgentStateReadModel>(_actualAgentStates);

	    public void SetActualAgentState(AgentStateReadModel agentStateReadModel)
        {
	        var person = FilteredPersons.FirstOrDefault(p => p.Id.GetValueOrDefault() == agentStateReadModel.PersonId);
	        if (person == null || person.Id == null)
                return;
	        var shouldUpdateUI = true;
			_actualAgentStates.AddOrUpdate((Guid) person.Id, agentStateReadModel, (key, oldState) =>
		        {
			        if (agentStateReadModel.ReceivedTime <= oldState.ReceivedTime)
			        {
				        shouldUpdateUI = false;
				        return oldState;
			        }
			        return agentStateReadModel;
		        });
			if (shouldUpdateUI)
				AgentstateUpdated?.Invoke(this, new CustomEventArgs<AgentStateReadModel>(agentStateReadModel));
        }

        public void SetFilteredPersons(IEnumerable<IPerson> filteredPersons)
        {
            FilteredPersons = filteredPersons;
        }

		public IEnumerable<IPerson> FilteredPersons
		{
			get; private set; 
		}

        public void VerifyDefaultStateGroupExists()
        {
            if (!RtaStateGroups.Any())
                throw new DefaultStateGroupException("The RTA state groups must be configured first.");
        }
    }
}