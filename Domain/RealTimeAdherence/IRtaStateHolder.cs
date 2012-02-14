using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence
{
    public interface IRtaStateHolder
    {
        event EventHandler<CustomEventArgs<IRtaState>> RtaStateCreated;
        IEnumerable<IRtaStateGroup> RtaStateGroups { get; }
        IEnumerable<IStateGroupActivityAlarm> StateGroupActivityAlarms { get; }
        ISchedulingResultStateHolder SchedulingResultStateHolder { get; }
        IDictionary<IPerson, IAgentState> AgentStates { get; }
        IList<ExternalLogOnPerson> ExternalLogOnPersons { get; }
        void CollectAgentStates(IEnumerable<IExternalAgentState> externalAgentStates);
        void UpdateCurrentLayers(DateTime timestamp, TimeSpan refreshRate);
        void AnalyzeAlarmSituations(DateTime timestamp);
        void SetFilteredPersons(IEnumerable<IPerson> filteredPersons);
        void InitializeSchedules();
        void VerifyDefaultStateGroupExists();
        void Initialize();
    }
}