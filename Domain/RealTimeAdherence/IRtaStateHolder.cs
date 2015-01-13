using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence
{
    public interface IRtaStateHolder
    {
        IEnumerable<IRtaStateGroup> RtaStateGroups { get; }
        IEnumerable<IStateGroupActivityAlarm> StateGroupActivityAlarms { get; }
        ISchedulingResultStateHolder SchedulingResultStateHolder { get; }
        IDictionary<Guid, AgentStateReadModel> ActualAgentStates { get; }
        void SetFilteredPersons(IEnumerable<IPerson> filteredPersons);
        void VerifyDefaultStateGroupExists();
        void Initialize();
        IEnumerable<IPerson> FilteredPersons { get; }
        void SetActualAgentState(AgentStateReadModel agentStateReadModel);
		event EventHandler<CustomEventArgs<AgentStateReadModel>> AgentstateUpdated;
    }
}