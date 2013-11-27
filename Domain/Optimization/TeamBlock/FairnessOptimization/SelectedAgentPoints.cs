using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
    public interface ISelectedAgentPoints
    {
        void AssignAgentPoints(IList<IPerson> persons);
        int GetPointOfAgent(IPerson person);
    }

    public class SelectedAgentPoints : ISelectedAgentPoints
    {
        private IDictionary<IPerson,int > _agentPoints;

        public SelectedAgentPoints()
        {
            _agentPoints = new Dictionary<IPerson, int>();
        }

        public void AssignAgentPoints(IList<IPerson> persons)
        {
            int i = 1;
            foreach (var person in persons.OrderBy(s=> s.Name.FirstName ))
            {
                _agentPoints.Add(person, i);
                i++;
            }
        }

        public int GetPointOfAgent(IPerson person)
        {
            return _agentPoints[person];
        }
    }
}
