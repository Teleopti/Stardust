using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.Rta.Persisters
{
	public class OrganizationReader : IOrganizationReader
	{
		private readonly INow _now;
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly ICurrentBusinessUnit _businessUnit;
		private readonly HardcodedSkillGroupingPageId _hardcodedSkillGroupingPageId;

		public OrganizationReader(
			ICurrentUnitOfWork unitOfWork,
			ICurrentBusinessUnit businessUnit, 
			HardcodedSkillGroupingPageId hardcodedSkillGroupingPageId, 
			INow now)
		{
			_unitOfWork = unitOfWork;
			_businessUnit = businessUnit;
			_hardcodedSkillGroupingPageId = hardcodedSkillGroupingPageId;
			_now = now;
		}

		public IEnumerable<OrganizationModel> Read()
		{
			return Read(null);
		}

		public IEnumerable<OrganizationModel> Read(IEnumerable<Guid> skillIds)
		{
			var querySkills = skillIds.EmptyIfNull().Any();

			var skillJoin = @"
					INNER JOIN ReadModel.GroupingReadOnly AS g
					ON a.PersonId = g.PersonId
					";

			var skillWhere = @"
					g.GroupId IN (:skillIds) AND
					g.PageId = :skillGroupingPageId AND 
					:now BETWEEN g.StartDate AND g.EndDate AND
					";

			var query = _unitOfWork.Current()
				.Session()
				.CreateSQLQuery($@"
					SELECT
						a.SiteId as SiteId, 
						MAX(a.SiteName) as SiteName,
						a.TeamId,
						MAX(a.TeamName) as TeamName
					FROM 
						ReadModel.AgentState AS a WITH(NOLOCK)
						{(querySkills ? skillJoin : "")}

					WHERE 
						{(querySkills ? skillWhere : "")}
						a.IsDeleted = 0 AND
						a.BusinessUnitId = :businessUnitId

					GROUP BY 
						a.BusinessUnitId,
						a.SiteId, 
						a.TeamId
					");

			query
				.SetParameter("businessUnitId", _businessUnit.Current().Id.Value)
				;

			if (querySkills)
				query
					.SetParameter("now", _now.UtcDateTime())
					.SetParameterList("skillIds", skillIds)
					.SetParameter("skillGroupingPageId", _hardcodedSkillGroupingPageId.Get())
					;

			var result = query
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.List()
				.Cast<internalModel>();

			return result
				.GroupBy(x => new { x.SiteId, x.SiteName })
				.Select(x => new OrganizationModel
				{
					Id = x.Key.SiteId,
					Name = x.Key.SiteName,
					Teams = x.Select(t =>
						new OrganizationTeamModel { Id = t.TeamId, Name = t.TeamName })
				})
				.ToArray();
		}

		private class internalModel
		{
			public Guid SiteId { get; set; }
			public string SiteName { get; set; }
			public Guid TeamId { get; set; }
			public string TeamName { get; set; }
		}

	}
}