using System;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Rta;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Rta
{
    public class FakeTeamAdherenceAggregator : ITeamAdherenceAggregator
    {
        private Dictionary<Guid, int> _data = new Dictionary<Guid, int>(); 

        public void Has(Guid teamId, int outOfAhderence)
        {
            _data.Add(teamId, outOfAhderence);
        }

        public int Aggregate(Guid teamId)
        {
            return _data[teamId];
        }
    }
}