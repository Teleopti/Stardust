using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SkillCombinationBpoTimeLineReader : ISkillCombinationBpoTimeLineReader
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		public SkillCombinationBpoTimeLineReader(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_currentBusinessUnit = currentBusinessUnit;
		}
		
		public IEnumerable<SkillCombinationResourceBpoTimelineModel> GetAllDataForBpoTimeline()
        		{
        			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
        			var result = _currentUnitOfWork.Current().Session()
        				.CreateSQLQuery(@"select bpo.Source, sum(r.Resources) AS Resources,  convert(datetime, convert(nvarchar, StartDateTime, 112)) AS OnDate, ImportFilename, r.InsertedOn as ImportedDateTime, p.Firstname, p.Lastname 
        							from ReadModel.SkillCombinationResourceBpo r 
        							INNER JOIN BusinessProcessOutsourcer bpo ON r.SourceId = bpo.Id
        							INNER JOIN Person p ON p.Id = r.PersonId
        							WHERE r.BusinessUnit = :bu 
        							GROUP BY bpo.Source, convert(datetime, convert(nvarchar, StartDateTime, 112)), ImportFilename, r.InsertedOn, p.Firstname, p.Lastname
        							ORDER BY bpo.Source, convert(datetime, convert(nvarchar, StartDateTime, 112))")
        				.SetParameter("bu", bu)
        				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(SkillCombinationResourceBpoTimelineModel)))
        				.List<SkillCombinationResourceBpoTimelineModel>();
        
        			return result.ToArray();
        		}

		public IEnumerable<SkillCombinationResourceBpoTimelineModel> GetBpoTimelineDataForSkill(Guid skillId)
		{
			var result = _currentUnitOfWork.Current().Session()
				.CreateSQLQuery(@"select bpo.Source, sum(r.Resources) AS Resources,  convert(datetime, convert(nvarchar, StartDateTime, 112)) AS OnDate, ImportFilename, r.InsertedOn as ImportedDateTime,  p.Firstname, p.Lastname 
							from ReadModel.SkillCombinationResourceBpo r 
							INNER JOIN ReadModel.SkillCombination k ON k.Id = r.SkillCombinationId
							INNER JOIN BusinessProcessOutsourcer bpo ON r.SourceId = bpo.Id
							INNER JOIN Person p ON p.Id = r.PersonId
							WHERE k.SkillId = :skillId 
							GROUP BY bpo.Source, convert(datetime, convert(nvarchar, StartDateTime, 112)), ImportFilename, r.InsertedOn, p.Firstname, p.Lastname
							ORDER BY bpo.Source, convert(datetime, convert(nvarchar, StartDateTime, 112))")
				.SetParameter("skillId", skillId)
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(SkillCombinationResourceBpoTimelineModel)))
				.List<SkillCombinationResourceBpoTimelineModel>();

			return result.ToArray();
		}

		public IEnumerable<SkillCombinationResourceBpoTimelineModel> GetBpoTimelineDataForSkillGroup(Guid skillGroupId)
		{
			var result = _currentUnitOfWork.Current().Session()
				.CreateSQLQuery(@"select bpo.Source, sum(r.Resources) AS Resources,  convert(datetime, convert(nvarchar, StartDateTime, 112)) AS OnDate, ImportFilename, r.InsertedOn as ImportedDateTime, p.Firstname, p.Lastname 
							from ReadModel.SkillCombinationResourceBpo r 
							INNER JOIN ReadModel.SkillCombination k ON k.Id = r.SkillCombinationId
							INNER JOIN BusinessProcessOutsourcer bpo ON r.SourceId = bpo.Id
							INNER JOIN Person p ON p.Id = r.PersonId
							INNER JOIN dbo.SkillAreaSkillCollection a ON k.SkillId = a.Skill
							WHERE a.SkillArea = :skillGroupId
							GROUP BY bpo.Source, convert(datetime, convert(nvarchar, StartDateTime, 112)), ImportFilename, r.InsertedOn, p.Firstname, p.Lastname
							ORDER BY bpo.Source, convert(datetime, convert(nvarchar, StartDateTime, 112))")
				.SetParameter("skillGroupId", skillGroupId)
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(SkillCombinationResourceBpoTimelineModel)))
				.List<SkillCombinationResourceBpoTimelineModel>();

			return result.ToArray();
		}

		public IEnumerable<SkillCombinationResourceBpoImportInfoModel> GetBpoImportInfoForSkill(Guid skillId, DateTime startDateTimeUtc, DateTime endDateTimeUtc)
		{
			var result = _currentUnitOfWork.Current().Session()
				.CreateSQLQuery(@"select bpo.Source, ImportFilename, r.InsertedOn as ImportedDateTime,  p.Firstname, p.Lastname 
							from ReadModel.SkillCombinationResourceBpo r 
							INNER JOIN ReadModel.SkillCombination k ON k.Id = r.SkillCombinationId
							INNER JOIN BusinessProcessOutsourcer bpo ON r.SourceId = bpo.Id
							INNER JOIN Person p ON p.Id = r.PersonId
							WHERE k.SkillId = :skillId
							AND r.StartDateTime  >= :startDateTimeUtc 
							AND r.EndDateTime < :endDateTimeUtc
							GROUP BY bpo.Source, ImportFilename, r.InsertedOn, p.Firstname, p.Lastname
							ORDER BY bpo.Source")
				.SetParameter("skillId", skillId)
				.SetParameter("startDateTimeUtc", startDateTimeUtc)
				.SetParameter("endDateTimeUtc", endDateTimeUtc)
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(SkillCombinationResourceBpoImportInfoModel)))
				.List<SkillCombinationResourceBpoImportInfoModel>();

			return result.ToArray();
		}

		public IEnumerable<SkillCombinationResourceBpoImportInfoModel> GetBpoImportInfoForSkillGroup(Guid skillGroupId, DateTime startDateTimeUtc, DateTime endDateTimeUtc)
		{
			var result = _currentUnitOfWork.Current().Session()
				.CreateSQLQuery(@"select bpo.Source, ImportFilename, r.InsertedOn as ImportedDateTime,  p.Firstname, p.Lastname 
							from ReadModel.SkillCombinationResourceBpo r 
							INNER JOIN ReadModel.SkillCombination k ON k.Id = r.SkillCombinationId
							INNER JOIN BusinessProcessOutsourcer bpo ON r.SourceId = bpo.Id
							INNER JOIN Person p ON p.Id = r.PersonId
							INNER JOIN dbo.SkillAreaSkillCollection a ON k.SkillId = a.Skill
							WHERE a.SkillArea = :skillGroupId
							AND r.StartDateTime between :startDateTimeUtc AND :endDateTimeUtc
							GROUP BY bpo.Source, ImportFilename, r.InsertedOn, p.Firstname, p.Lastname
							ORDER BY bpo.Source")
				.SetParameter("skillGroupId", skillGroupId)
				.SetParameter("startDateTimeUtc", startDateTimeUtc)
				.SetParameter("endDateTimeUtc", endDateTimeUtc)
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(SkillCombinationResourceBpoImportInfoModel)))
				.List<SkillCombinationResourceBpoImportInfoModel>();

			return result.ToArray();
		}
	}
}