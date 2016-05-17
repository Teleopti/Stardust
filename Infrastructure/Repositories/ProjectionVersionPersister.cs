using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ProjectionVersionPersister : IProjectionVersionPersister
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public ProjectionVersionPersister(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<ProjectionVersion> LockAndGetVersions(Guid personId, DateOnly from, DateOnly to)
		{
			var dates = from.DateRange(to).Select(x => x.Date.ToString("yyyy-MM-dd"));
			var dateString = string.Join(" UNION ", dates.Select(x => $"SELECT '{x}'"));
			_unitOfWork.Session().CreateSQLQuery(
				$@"MERGE INTO [dbo].ProjectionVersion WITH (XLOCK) AS T  
						USING (
						{dateString}
					) AS S ( 
						[Date]  
					) 
					ON
						T.Person = :Person AND
						T.Date = S.Date
					WHEN NOT MATCHED THEN
					INSERT
					(
						Person,
						Date,
						[Version]
					) VALUES(
						:Person,
						S.Date,
						1
					)
					WHEN MATCHED THEN
					UPDATE SET
						[Version] = [Version] + 1
						; ")
				.SetGuid("Person", personId)
				.ExecuteUpdate();

			var formattedFrom = from.Date.ToString("yyyy-MM-dd");
			var formattedTo = from.Date.ToString("yyyy-MM-dd");
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(
				$@"SELECT Date, Version 
					FROM [dbo].ProjectionVersion WITH (NOLOCK) 
					WHERE Date BETWEEN '{formattedFrom}' AND '{formattedTo}'
					AND Person = '{personId}'")
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalVersion)))
				.List<ProjectionVersion>();
		}

		private class internalVersion : ProjectionVersion
		{
			public new DateTime Date
			{
				set { base.Date = new DateOnly(value); }
			}
		}
	}
}
