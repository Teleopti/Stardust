using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeNumberOfAgentsInSiteReader : INumberOfAgentsInSiteReader
	{
		private readonly List<numberOfAgentsForSite> _data = new List<numberOfAgentsForSite>();
		
		public void Has(Guid siteId, int numberOfAgents)
		{
			Has(siteId, Guid.Empty, numberOfAgents);
		}

		public void Has(Guid siteId, Guid skillId, int numberOfAgents)
		{
			_data.Add(
				new numberOfAgentsForSite
				{
					SiteId = siteId,
					SkillId = skillId,
					NumberOfAgents = numberOfAgents
				});
		}

		public IDictionary<Guid, int> FetchNumberOfAgents(IEnumerable<ISite> sites)
		{
			return _data.ToDictionary(x => x.SiteId, y => y.NumberOfAgents);
		}

		public IDictionary<Guid, int> ForSkills(IEnumerable<Guid> sites, IEnumerable<Guid> skillIds)
		{
			return (
				from siteId in sites
				from skillId in skillIds
				from model in _data
				where model.SiteId == siteId &&
					  model.SkillId == skillId
				select new 
				{
					SiteId = siteId,
					NumberOfAgents = model.NumberOfAgents
				})
				.ToDictionary(x => x.SiteId, y => y.NumberOfAgents);
		}

		private class numberOfAgentsForSite
		{
			public Guid SiteId { get; set; }
			public Guid SkillId { get; set; }
			public int NumberOfAgents { get; set; }
		}
	}
}
