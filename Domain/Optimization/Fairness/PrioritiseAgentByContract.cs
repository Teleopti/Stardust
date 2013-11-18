using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.Fairness
{
    public interface IPrioritiseAgentByContract
    {
        int HigestPriority { get; }
        int LowestPriority { get; }
        IDictionary<int, IPerson> GetPriortiseAgentByName(IList<IPerson> personList);
        IDictionary<int, IPerson> GetPriortiseAgentByStartDate(IList<IPerson> personList);
        IPerson PersonOnPriority(int priority);
    }

    public class PrioritiseAgentByContract : IPrioritiseAgentByContract
    {
        private Dictionary<int, IPerson> _result = new Dictionary<int, IPerson>();

        public IDictionary<int, IPerson> GetPriortiseAgentByName(IList<IPerson> personList)
        {
            var result = new Dictionary<int, IPerson>();
            int priorityIndex = 1;
            foreach (IPerson person in personList.OrderByDescending(s => s.Name.FirstName))
            {
                result.Add(priorityIndex, person);
                priorityIndex++;
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

        public int HigestPriority
        {
            get { return _result.Keys.Max(); }
        }

        public int LowestPriority
        {
            get { return _result.Keys.Min(); }
        }

        public IPerson PersonOnPriority(int priority)
        {
            return _result[priority];
        }
    }
}