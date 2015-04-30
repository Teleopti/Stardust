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
		private readonly ConcurrentDictionary<Guid, IActualAgentState> _actualAgentStates = new ConcurrentDictionary<Guid, IActualAgentState>();
		
        public RtaStateHolder(ISchedulingResultStateHolder schedulingResultStateHolder, IRtaStateGroupRepository rtaStateGroupRepository, IStateGroupActivityAlarmRepository stateGroupActivityAlarmRepository)
        {
            InParameter.NotNull("schedulingResultStateHolder", schedulingResultStateHolder);
            InParameter.NotNull("rtaStateGroupProvider", rtaStateGroupRepository);
            InParameter.NotNull("stateGroupActivityAlarmProvider", stateGroupActivityAlarmRepository);

            _schedulingResultStateHolder = schedulingResultStateHolder;
            _rtaStateGroupRepository = rtaStateGroupRepository;
        }

        public void Initialize()
        {
            RtaStateGroups = _rtaStateGroupRepository.LoadAllCompleteGraph();
        }

	    public IEnumerable<IRtaStateGroup> RtaStateGroups { get; private set; }


	    public ISchedulingResultStateHolder SchedulingResultStateHolder
        {
            get { return _schedulingResultStateHolder; }
        }

	    public event EventHandler<CustomEventArgs<IActualAgentState>> AgentstateUpdated;

        public IDictionary<Guid, IActualAgentState> ActualAgentStates
        {
            get { return new ReadOnlyDictionary<Guid, IActualAgentState>(_actualAgentStates); }
        }

        public void SetActualAgentState(IActualAgentState actualAgentState)
        {
	        var person = FilteredPersons.FirstOrDefault(p => p.Id.GetValueOrDefault() == actualAgentState.PersonId);
	        if (person == null || person.Id == null)
                return;
	        _actualAgentStates.AddOrUpdate((Guid) person.Id, actualAgentState, (key, oldState) =>
		        {
			        if (oldState.ReceivedTime > actualAgentState.ReceivedTime)
				        return oldState;
			        return actualAgentState;
		        });
	        var handler = AgentstateUpdated;
			if (handler != null)
				handler.Invoke(this, new CustomEventArgs<IActualAgentState>(actualAgentState));
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