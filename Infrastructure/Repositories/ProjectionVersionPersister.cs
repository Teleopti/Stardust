using System;
using System.Collections.Generic;
using System.Linq;
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

		public void Upsert(Guid personId, IEnumerable<DateOnly> dates)
		{
			var dateString = string.Join(" UNION ", dates.Select(x=> $"SELECT '{x.Date}'"));
			_unitOfWork.Session().CreateSQLQuery(
				$@"MERGE INTO [dbo].ProjectionVersion AS T  
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
						0
					)
					WHEN MATCHED THEN
					UPDATE SET
						[Version] = [Version] + 1
						; ")
				.SetGuid("Person", personId)
				.ExecuteUpdate();
		}
	}
}
