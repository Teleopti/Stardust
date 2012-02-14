using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence
{
    public class AgentStateBatch : IAgentStateBatch
    {
        private readonly BatchIdentifier _batchIdentifier;
        private readonly IList<IPerson> _personCollection = new List<IPerson>();

        public AgentStateBatch(BatchIdentifier batchIdentifier)
        {
            _batchIdentifier = batchIdentifier;
        }

        public BatchIdentifier BatchIdentifier
        {
            get { return _batchIdentifier; }
        }

        public ReadOnlyCollection<IPerson> PersonCollection
        {
            get { return new ReadOnlyCollection<IPerson>(_personCollection); }
        }

        public void AddPerson(IPerson person)
        {
            if (!_personCollection.Contains(person))
                _personCollection.Add(person);
        }

        public IEnumerable<IPerson> CompareWithPreviousBatch(IAgentStateBatch previousBatch)
        {
            return previousBatch.PersonCollection.Except(_personCollection);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public struct BatchIdentifier
    {
        public DateTime BatchTimestamp { get; set; }
        public int DataSourceId { get; set; }
    }
}