using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence
{
    public interface IAgentStateBatch
    {
        BatchIdentifier BatchIdentifier { get; }
        ReadOnlyCollection<IPerson> PersonCollection { get; }
        void AddPerson(IPerson person);
        IEnumerable<IPerson> CompareWithPreviousBatch(IAgentStateBatch previousBatch);
    }
}