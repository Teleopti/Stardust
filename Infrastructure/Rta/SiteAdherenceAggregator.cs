using System;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public interface ISiteAdherenceAggregator
	{
	}
	
	public class SiteAdherenceAggregator : ISiteAdherenceAggregator
	{
		private readonly IStatisticRepository _statisticRepository;
		private readonly IPersonOrganizationReader _personOrganization;

		public SiteAdherenceAggregator(IStatisticRepository statisticRepository, IPersonOrganizationReader personOrganization)
		{
			_statisticRepository = statisticRepository;
			_personOrganization = personOrganization;
		}

		public int Aggregate(Guid siteId)
		{
			var personIds = _personOrganization.LoadAll().Where(x => x.SiteId == siteId).Select(x => x.PersonId).ToList();
			var lastStates = _statisticRepository.LoadLastAgentState(personIds);
			return lastStates.Count(x => x.StaffingEffect > 0);
		}
	}
}