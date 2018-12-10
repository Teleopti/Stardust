using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Configuration
{
    public interface IRtaStateHolder
    {
        IEnumerable<IRtaStateGroup> RtaStateGroups { get; }
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