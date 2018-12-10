using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

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
			var dates = from.DateRange(to).Select((d,i) => new {D = d, name = "d" + i}).ToArray();
			var sqlQuery = _unitOfWork.Session().CreateSQLQuery(
				$@"MERGE INTO [dbo].ProjectionVersion WITH (XLOCK) AS T  
						USING (
						SELECT * FROM (VALUES {string.Join(",",dates.Select(d => "(:" + d.name + ")"))}) AS x (d)
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
						; ");

			foreach (var d in dates)
			{
				sqlQuery.SetDateOnly(d.name, d.D);
			}
			sqlQuery
				.SetGuid("Person", personId)
				.ExecuteUpdate();

			return _unitOfWork.Current().Session()
				.CreateSQLQuery(
				$@"SELECT Date, Version 
					FROM [dbo].ProjectionVersion WITH (NOLOCK) 
					WHERE Date BETWEEN :From AND :To
					AND Person = :Person")
				.SetDateOnly("From", from)
				.SetDateOnly("To", to)
				.SetGuid("Person", personId)
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
