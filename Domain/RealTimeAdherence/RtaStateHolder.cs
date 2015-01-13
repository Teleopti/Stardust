using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence
{
    public class RtaStateHolder : IRtaStateHolder
    {
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly IRtaStateGroupRepository _rtaStateGroupRepository;
        private readonly IStateGroupActivityAlarmRepository _stateGroupActivityAlarmRepository;
		private readonly ConcurrentDictionary<Guid, AgentStateReadModel> _actualAgentStates = new ConcurrentDictionary<Guid, AgentStateReadModel>();
		
        public RtaStateHolder(ISchedulingResultStateHolder schedulingResultStateHolder, IRtaStateGroupRepository rtaStateGroupRepository, IStateGroupActivityAlarmRepository stateGroupActivityAlarmRepository)
        {
            InParameter.NotNull("schedulingResultStateHolder", schedulingResultStateHolder);
            InParameter.NotNull("rtaStateGroupProvider", rtaStateGroupRepository);
            InParameter.NotNull("stateGroupActivityAlarmProvider", stateGroupActivityAlarmRepository);

            _schedulingResultStateHolder = schedulingResultStateHolder;
            _rtaStateGroupRepository = rtaStateGroupRepository;
            _stateGroupActivityAlarmRepository = stateGroupActivityAlarmRepository;
        }

        public void Initialize()
        {
            RtaStateGroups = _rtaStateGroupRepository.LoadAllCompleteGraph();
            StateGroupActivityAlarms = _stateGroupActivityAlarmRepository.LoadAllCompleteGraph();
        }

	    public IEnumerable<IRtaStateGroup> RtaStateGroups { get; private set; }

	    public IEnumerable<IStateGroupActivityAlarm> StateGroupActivityAlarms { get; private set; }

	    public ISchedulingResultStateHolder SchedulingResultStateHolder
        {
            get { return _schedulingResultStateHolder; }
        }

	    public event EventHandler<CustomEventArgs<AgentStateReadModel>> AgentstateUpdated;

        public IDictionary<Guid, AgentStateReadModel> ActualAgentStates
        {
            get { return new ReadOnlyDictionary<Guid, AgentStateReadModel>(_actualAgentStates); }
        }

        public void SetActualAgentState(AgentStateReadModel agentStateReadModel)
        {
	        var person = FilteredPersons.FirstOrDefault(p => p.Id.GetValueOrDefault() == agentStateReadModel.PersonId);
	        if (person == null || person.Id == null)
                return;
	        _actualAgentStates.AddOrUpdate((Guid) person.Id, agentStateReadModel, (key, oldState) =>
		        {
			        if (oldState.ReceivedTime > agentStateReadModel.ReceivedTime)
				        return oldState;

			        return agentStateReadModel;
		        });
	        var handler = AgentstateUpdated;
			if (handler != null)
				handler.Invoke(this, new CustomEventArgs<AgentStateReadModel>(agentStateReadModel));
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