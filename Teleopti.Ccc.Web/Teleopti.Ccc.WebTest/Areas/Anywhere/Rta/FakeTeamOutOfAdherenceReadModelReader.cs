using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Rta
{
    public class FakeTeamOutOfAdherenceReadModelReader : ITeamOutOfAdherenceReadModelReader
    {
        private IEnumerable<TeamOutOfAdherenceReadModel> _data = Enumerable.Empty<TeamOutOfAdherenceReadModel>();

        public void Has(Guid siteId, Guid teamId, int outOfAdherence)
        {
            _data =
                _data.Concat(new[]
                {new TeamOutOfAdherenceReadModel {SiteId = siteId, TeamId = teamId, Count = outOfAdherence}});
        }

        public IEnumerable<TeamOutOfAdherenceReadModel> Read(Guid siteId)
        {
            return _data.Where(x => x.SiteId == siteId);
        }
        
    }
}