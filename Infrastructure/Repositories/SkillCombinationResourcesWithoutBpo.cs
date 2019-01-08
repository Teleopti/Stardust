using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SkillCombinationResourcesWithoutBpoToggleOff : ISkillCombinationResourcesWithoutBpo
	{
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public SkillCombinationResourcesWithoutBpoToggleOff(ICurrentBusinessUnit currentBusinessUnit, ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentBusinessUnit = currentBusinessUnit;
			_currentUnitOfWork = currentUnitOfWork;
		}

		public IEnumerable<SkillCombinationResource> Load(DateTimePeriod period)
		{
			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			var result = _currentUnitOfWork.Current().Session()
				.CreateSQLQuery(
					@"
 SELECT  SkillCombinationId, StartDateTime, EndDateTime, Resource, c.SkillId
 FROM
(SELECT  SkillCombinationId, StartDateTime, EndDateTime, SUM(Resource) AS Resource FROM 
 (SELECT r.SkillCombinationId, r.StartDateTime, r.EndDateTime, r.Resource
 from [ReadModel].[SkillCombinationResource] r WHERE r.BusinessUnit = :bu
AND r.StartDateTime < :endDateTime AND r.EndDateTime > :startDateTime
 UNION ALL
 SELECT d.SkillCombinationId, d.StartDateTime, d.EndDateTime, d.DeltaResource AS Resource
 from [ReadModel].[SkillCombinationResourceDelta] d WHERE d.BusinessUnit = :bu
AND d.StartDateTime < :endDateTime AND d.EndDateTime > :startDateTime)
 AS tmp
 GROUP BY tmp.SkillCombinationId, tmp.StartDateTime, tmp.EndDateTime) AS summary
 INNER JOIN [ReadModel].[SkillCombination] c ON summary.SkillCombinationId = c.Id")

				.SetDateTime("startDateTime", period.StartDateTime)
				.SetDateTime("endDateTime", period.EndDateTime)
				.SetParameter("bu", bu)
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(RawSkillCombinationResource)))
				.List<RawSkillCombinationResource>();

			var mergedResult =
				result.GroupBy(x => new { x.SkillCombinationId, x.StartDateTime, x.EndDateTime, x.Resource })
					.Select(
						x =>
							new SkillCombinationResourceWithCombinationId
							{
								StartDateTime = x.Key.StartDateTime.Utc(),
								EndDateTime = x.Key.EndDateTime.Utc(),
								Resource = x.Key.Resource < 0 ? 0 : x.Key.Resource,
								SkillCombinationId = x.Key.SkillCombinationId,
								SkillCombination = x.Select(s => s.SkillId).OrderBy(s => s).ToArray()
							});

			return mergedResult;
		}
	}

	public class SkillCombinationResourcesWithoutBpoToggleOn : ISkillCombinationResourcesWithoutBpo
	{
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public SkillCombinationResourcesWithoutBpoToggleOn(ICurrentBusinessUnit currentBusinessUnit, ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentBusinessUnit = currentBusinessUnit;
			_currentUnitOfWork = currentUnitOfWork;
		}

		public IEnumerable<SkillCombinationResource> Load(DateTimePeriod period)
		{
			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			var result = _currentUnitOfWork.Current().Session()
				.CreateSQLQuery(
					@"SELECT SkillCombinationId, StartDateTime, EndDateTime, sum([Resource]) as [Resource], SkillId
FROM
(
       SELECT r.SkillCombinationId, r.StartDateTime, r.EndDateTime, sum(r.Resource) as [Resource],c.SkillId
       from [ReadModel].[SkillCombinationResource] r
       INNER JOIN [ReadModel].[SkillCombination] c ON r.SkillCombinationId = c.Id
       WHERE r.BusinessUnit = :bu
       AND r.StartDateTime < :endDateTime AND r.EndDateTime > :startDateTime
       GROUP BY r.SkillCombinationId, r.StartDateTime, r.EndDateTime, c.SkillId
       UNION ALL
       SELECT d.SkillCombinationId, d.StartDateTime, d.EndDateTime, sum(d.DeltaResource) AS [Resource],c.SkillId
       from [ReadModel].[SkillCombinationResourceDelta] d
       INNER JOIN [ReadModel].[SkillCombination] c ON d.SkillCombinationId = c.Id
       WHERE d.BusinessUnit = :bu
       AND d.StartDateTime < :endDateTime AND d.EndDateTime > :startDateTime
       GROUP BY d.SkillCombinationId, d.StartDateTime, d.EndDateTime, c.SkillId
) summary
GROUP BY SkillCombinationId, StartDateTime, EndDateTime, SkillId
")

				.SetDateTime("startDateTime", period.StartDateTime)
				.SetDateTime("endDateTime", period.EndDateTime)
				.SetParameter("bu", bu)
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(RawSkillCombinationResource)))
				.List<RawSkillCombinationResource>();

			var mergedResult =
				result.GroupBy(x => new { x.SkillCombinationId, x.StartDateTime, x.EndDateTime, x.Resource })
					.Select(
						x =>
							new SkillCombinationResourceWithCombinationId
							{
								StartDateTime = x.Key.StartDateTime.Utc(),
								EndDateTime = x.Key.EndDateTime.Utc(),
								Resource = x.Key.Resource < 0 ? 0 : x.Key.Resource,
								SkillCombinationId = x.Key.SkillCombinationId,
								SkillCombination = x.Select(s => s.SkillId).OrderBy(s => s).ToArray()
							});

			return mergedResult;
		}
	}

	public interface ISkillCombinationResourcesWithoutBpo
	{
		IEnumerable<SkillCombinationResource> Load(DateTimePeriod period);
	}
}