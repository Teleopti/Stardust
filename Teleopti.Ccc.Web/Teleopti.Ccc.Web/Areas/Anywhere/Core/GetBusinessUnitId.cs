using System;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class GetBusinessUnitId : IGetBusinessUnitId
	{
		private readonly ITeamRepository _teamRepository;

		public GetBusinessUnitId( ITeamRepository teamRepository)
		{
			_teamRepository = teamRepository;
		}

		public Guid Get(string teamId)
		{
			var team = _teamRepository.Get(new Guid(teamId));
			return team.BusinessUnitExplicit.Id.GetValueOrDefault();
		}

	}
}