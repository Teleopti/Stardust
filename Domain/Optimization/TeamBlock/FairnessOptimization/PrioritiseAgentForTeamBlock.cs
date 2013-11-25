using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
    public interface IPrioritiseAgentForTeamBlock
    {
        int AveragePriority { get;  }
        IDictionary<int, IPerson> GetPriortiseAgentByName(IList<IPerson> personList);
        IDictionary<int, IPerson> GetPriortiseAgentByStartDate(IList<IPerson> personList);
        IPerson PersonOnPriority(int priority);
        void Clear();
    }

    public class PrioritiseAgentForTeamBlock : IPrioritiseAgentForTeamBlock
    {
        private readonly ISelectedAgentPoints _selectedAgentPoints;
        private Dictionary<int, IPerson> _result = new Dictionary<int, IPerson>();

        public PrioritiseAgentForTeamBlock(ISelectedAgentPoints selectedAgentPoints)
        {
            _selectedAgentPoints = selectedAgentPoints;
        }

        public int AveragePriority
        {
            get { return (int) _result.Keys.Average(); }
        }

        public IDictionary<int, IPerson> GetPriortiseAgentByName(IList<IPerson> personList)
        {
            var result = new Dictionary<int, IPerson>();
            foreach (IPerson person in personList.OrderByDescending(s => s.Name.FirstName))
            {
                result.Add(_selectedAgentPoints.GetPointOfAgent(person), person);
            }
            _result = result;
            return result;
        }

        public IDictionary<int, IPerson> GetPriortiseAgentByStartDate(IList<IPerson> personList)
        {
            //var result = new Dictionary<int, IPerson>();
            //var personByDate = new Dictionary<IPerson, DateOnly>();
            //foreach (var person in personList)
            //{
            //    var startDate =  person.PersonPeriodCollection.Min().StartDate;
            //    personByDate.Add(person,startDate );
            //}
            //int priorityIndex = 0;
            //foreach (var person in  personByDate.OrderByDescending(s => s.Value).Select(f => f.Key))
            //{
            //    result.Add(priorityIndex, person);
            //    priorityIndex++;
            //}
            //return result;
            return GetPriortiseAgentByName(personList);
        }
        
        public IPerson PersonOnPriority(int priority)
        {
            return _result[priority];
        }

        public void Clear()
        {
            _result.Clear();
        }
    }
}