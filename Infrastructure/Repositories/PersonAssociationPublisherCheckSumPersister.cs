using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PersonAssociationPublisherCheckSumPersister : IPersonAssociationPublisherCheckSumPersister
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public PersonAssociationPublisherCheckSumPersister(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<PersonAssociationCheckSum> Get()
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery("SELECT * FROM dbo.PersonAssociationCheckSum WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean<PersonAssociationCheckSum>())
				.List<PersonAssociationCheckSum>();
		}

		public PersonAssociationCheckSum Get(Guid personId)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery("SELECT * FROM dbo.PersonAssociationCheckSum WITH (NOLOCK) WHERE PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.SetResultTransformer(Transformers.AliasToBean<PersonAssociationCheckSum>())
				.UniqueResult<PersonAssociationCheckSum>();
		}

		public void Persist(PersonAssociationCheckSum checkSum) => Persist(checkSum.AsArray());

		public void Persist(IEnumerable<PersonAssociationCheckSum> checkSums)
		{
			var toBeDeleted = checkSums.Where(x => x.CheckSum == 0).Select(x => x.PersonId).ToArray();
			if (toBeDeleted.Any())
			{
				_unitOfWork.Current().Session()
					.CreateSQLQuery(@"
DELETE dbo.PersonAssociationCheckSum 
WHERE
PersonId IN (:PersonIds)")
					.SetParameterList("PersonIds", toBeDeleted)
					.ExecuteUpdate();
			}

			var mergedCheckSums = checkSums.Where(x => x.CheckSum != 0).ToArray();
			if (mergedCheckSums.IsEmpty())
				return;
			
			var sqlValues = mergedCheckSums.Select((m, i) =>
				$@"
(
:PersonId{i},
:CheckSum{i}
)").Aggregate((current, next) => current + ", " + next);

			var query = _unitOfWork.Current()
				.Session().CreateSQLQuery($@"
MERGE INTO dbo.PersonAssociationCheckSum AS T
	USING (
		VALUES
		{sqlValues}
	) AS S (
			PersonId,
			CheckSum
		)
	ON 
		T.PersonId = S.PersonId
	WHEN NOT MATCHED THEN
		INSERT
		(
			PersonId,
			CheckSum
		) VALUES (
			S.PersonId,
			S.CheckSum
		)
	WHEN MATCHED THEN
		UPDATE SET
			CheckSum = S.CheckSum
		;");
			
			mergedCheckSums.Select((m, i) => new {m, i})
				.ForEach(x =>
				{
					var i = x.i;
					var mapping = x.m;
					query
						.SetParameter("PersonId" + i, mapping.PersonId)
						.SetParameter("CheckSum" + i, mapping.CheckSum);
				});

			query.ExecuteUpdate();
		}		
	}
}